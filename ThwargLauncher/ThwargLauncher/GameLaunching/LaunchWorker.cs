using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    class LaunchWorker
    {
        // Members passed in construtor
        BackgroundWorker _worker;
        private readonly GameSessionMap _gameSessionMap;
        // Rest of members
        private object _launchTimingLock = new object();
        private DateTime _launchRequestedTimeUtc = DateTime.MinValue;
        private DateTime _lastLaunchInitiatedUtc = DateTime.MinValue;
        private int _serverIndex;
        private int _serverTotal;

        public delegate void ReportLaunchItemStatusHandler(GameStatusNotice statusNotice, LaunchItem launchItem, int serverIndex, int serverTotal);
        public event ReportLaunchItemStatusHandler ReportLaunchItemStatusEvent;
        private void FireReportLaunchItemStatusEvent(GameStatusNotice statusNotice, LaunchItem launchItem)
        {
            if (ReportLaunchItemStatusEvent == null) { return; }
            ReportLaunchItemStatusEvent(statusNotice, launchItem, _serverIndex, _serverTotal);
        }
        public delegate void ReportAccountStatusHandler(ServerAccountStatusEnum accountStatus, LaunchItem launchItem);
        public event ReportAccountStatusHandler ReportAccountStatusEvent;
        private void FireReportAccountStatusEvent(ServerAccountStatusEnum accountStatus, LaunchItem launchItem) { if (ReportAccountStatusEvent != null) { ReportAccountStatusEvent(accountStatus, launchItem); } }

        public delegate void ProgressChangedHandler(object sender, ProgressChangedEventArgs e);
        public event ProgressChangedHandler ProgressChangedEvent;
        private void FireProgressChangedEvent(object sender, ProgressChangedEventArgs e) { if (ProgressChangedEvent != null) { ProgressChangedEvent(sender, e); } }

        public delegate void WorkerCompletedHandler(object sender, RunWorkerCompletedEventArgs e);
        public event WorkerCompletedHandler WorkerCompletedEvent;
        private void FireWorkerCompletedEvent(object sender, RunWorkerCompletedEventArgs e) { if (WorkerCompletedEvent != null) { WorkerCompletedEvent(sender, e); } }

        public LaunchWorker(BackgroundWorker worker, GameSessionMap gameSessionMap)
        {
            _worker = worker;
            _gameSessionMap = gameSessionMap;
            Initialize();
        }

        private class WorkerArgs
        {
            public System.Collections.Concurrent.ConcurrentQueue<LaunchItem> ConcurrentLaunchQueue;
            public string ClientExeLocation;
        }

        private void Initialize()
        {
            WireUpBackgroundWorker();
        }

        private void WireUpBackgroundWorker()
        {
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += (s, e) => FireProgressChangedEvent(s, e);
            _worker.RunWorkerCompleted += (s, e) => FireWorkerCompletedEvent(s, e);
        }

        public void LaunchQueue(ConcurrentQueue<LaunchItem> launchQueue, string clientExeLocation)
        {
            WorkerArgs args = new WorkerArgs() { ConcurrentLaunchQueue = launchQueue, ClientExeLocation = clientExeLocation };
            _worker.RunWorkerAsync(args);
        }

        public bool IsLaunchDue()
        {
            lock (_launchTimingLock)
            {
                if (_launchRequestedTimeUtc > _lastLaunchInitiatedUtc)
                {
                    return true;
                }
                else
                {
                    var elapsed = DateTime.UtcNow - _lastLaunchInitiatedUtc;
                    if (elapsed.TotalSeconds > ConfigSettings.GetConfigInt("RelaunchIntervalSeconds", 60))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RequestImmediateLaunch()
        {
            lock (_launchTimingLock)
            {
                if (_launchRequestedTimeUtc > DateTime.UtcNow)
                {
                    _launchRequestedTimeUtc = DateTime.UtcNow;
                }
            }
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs args = (e.Argument as WorkerArgs);
            if (args == null) { return; }
            lock (_launchTimingLock)
            {
                _lastLaunchInitiatedUtc = DateTime.UtcNow;
            }
            _serverIndex = 0;
            System.Collections.Concurrent.ConcurrentQueue<LaunchItem> globalQueue = args.ConcurrentLaunchQueue;
            _serverTotal = globalQueue.Count;
            if (_serverTotal == 0) { return; }

            LaunchItem launchItem = null;
            var accountLaunchTimes = _gameSessionMap.GetLaunchAccountTimes();

            while (globalQueue.TryDequeue(out launchItem))
            {
                int threadDelayMs = ConfigSettings.GetConfigInt("ThreadGameLaunchDelayMs", 100);
                Thread.Sleep(threadDelayMs);
                new Thread((x) =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    launchGameFromItem(args, (LaunchItem)x, accountLaunchTimes);
                }).Start(launchItem);

                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        void launchGameFromItem(WorkerArgs args, LaunchItem launchItem, Dictionary<string, DateTime> accountLaunchTimes)
        {
            Logger.WriteDebug("launchGameFromItem on thread {0}: Account={1}, Server={2}, Char={3}",
                Thread.CurrentThread.ManagedThreadId,
                launchItem.AccountName, launchItem.ServerName, launchItem.CharacterSelected);
            LaunchManager mgr = new LaunchManager(args.ClientExeLocation, launchItem, accountLaunchTimes);
            mgr.ReportStatusEvent += (status, item) => FireReportLaunchItemStatusEvent(status, item);
            LaunchManager.LaunchManagerResult launchResult;
            GameSession session = null;
            try
            {
                session = _gameSessionMap.StartLaunchingSession(launchItem.ServerName, launchItem.AccountName);
                FireReportAccountStatusEvent(ServerAccountStatusEnum.Starting, launchItem);
                launchResult = mgr.LaunchGameHandlingDelaysAndTitles(_worker);
            }
            finally
            {
                _gameSessionMap.EndLaunchingSession(launchItem.ServerName, launchItem.AccountName);
            }

            if (launchResult.Success)
            {
                ++_serverIndex;
                // Let's just wait for game monitor to check if the character list changed
                // b/c the AccountManager is subscribed for that event
                //CallUiNotifyAvailableCharactersChanged(); // Pick up any characters - experimental 2017-04-10
                // CallUiLoadUserAccounts(); // Pick up any characters - before 2017-04-10
                _gameSessionMap.StartSessionWatcher(session);
                session.WindowHwnd = launchResult.Hwnd;
                // session.ProcessId is already populated
                FireReportLaunchItemStatusEvent(GameStatusNotice.CreateSuccess("Launched"), launchItem);
            }
        }
    }
}
