using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    class GameMonitor
    {
        private static object _locker = new object();

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private Dictionary<int, GameStatus> _statusMap = new Dictionary<int, GameStatus>();
        private TimeSpan _liveInterval = new TimeSpan(0, 1, 0); // must be written this recently to be alive
        private DateTime _lastCleanupUtc = DateTime.MinValue;
        private TimeSpan _cleanupInterval = new TimeSpan(0, 5, 0); // 5 minutes
        private bool _rereadRequested = false; // cross-thread access
        private bool _isWorking = false; // reentrancy guard

        public enum GameChangeType { StartGame, EndGame, ChangeGame };
        public delegate void GameChangeHandler(GameChangeType changeType, GameStatus gameStatus);
        public event GameChangeHandler GameChangeEvent;

        public void Start() // main thread
        {
            int intervalMilliseconds = 3000;
            intervalMilliseconds = 20000; // TODO
            _timer.Interval = intervalMilliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }
        public void Stop() // main thread
        {
            _timer.Stop();
        }
        public void QueueReread() // main thread
        {
            lock (_locker)
            {
                if (!_rereadRequested)
                {
                    _rereadRequested = true;
                }
            }
        }
        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isWorking) { return; }
            _isWorking = true;
            if (ShouldWeCleanup())
            {
                CleanupOldProcessFiles();
            }
            ReadProcessFiles();
            Log.WriteError("Testing timer");
            _isWorking = false;
        }
        private bool ShouldWeCleanup()
        {
            if (_lastCleanupUtc == DateTime.MinValue || DateTime.UtcNow - _lastCleanupUtc > _cleanupInterval)
            {
                return true;
            }
            lock (_locker)
            {
                if (_rereadRequested)
                {
                    return true;
                }
            }
            return false;
        }
        private void ReadProcessFiles()
        {
            foreach (var gameStatus in _statusMap.Values)
            {
                string heartbeatFile = gameStatus.ProcessStatusFilepath;
                var response = MagFilter.LaunchControl.GetHeartbeatStatus(heartbeatFile);
                if (response.IsValid)
                {
                    if (gameStatus.AccountName == null)
                    {
                        // newly found
                        gameStatus.AccountName = response.Status.AccountName;
                        gameStatus.ServerName = response.Status.ServerName;
                        gameStatus.CharacterName = response.Status.CharacterName;
                        NotifyGameChange(GameChangeType.StartGame, gameStatus);
                    }
                    if (gameStatus.AccountName != response.Status.AccountName
                        || gameStatus.ServerName != response.Status.ServerName)
                    {
                        Log.WriteError(string.Format("Account/Server change in heartbeat file!: {0}", heartbeatFile));
                        gameStatus.AccountName = response.Status.AccountName;
                        gameStatus.ServerName = response.Status.ServerName;
                        NotifyGameChange(GameChangeType.ChangeGame, gameStatus);
                    }
                    if (gameStatus.CharacterName != response.Status.CharacterName)
                    {
                        gameStatus.CharacterName = response.Status.CharacterName;
                        NotifyGameChange(GameChangeType.ChangeGame, gameStatus);
                    }
                }
                else
                {
                    Log.WriteError(string.Format("Invalid contents in heartbeat file: {0}", heartbeatFile));
                }
            }
        }
        private void CleanupOldProcessFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(MagFilter.FileLocations.GetRunningFolder());
            var filepathsToDelete = new List<string>();
            foreach (var fileInfo in dir.EnumerateFiles())
            {
                int processId = 0;
                if (fileInfo.Extension == ".txt")
                {
                    processId = MagFilter.FileLocations.GetProcessIdFromProcessDllToExeFilepath(fileInfo.Name);
                    if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < _liveInterval)
                    {
                    }
                    else
                    {
                        // obsolete heartbeat file
                        if (_statusMap.ContainsKey(processId))
                        {
                            var gameStatus = _statusMap[processId];
                            _statusMap.Remove(processId);
                            NotifyGameChange(GameChangeType.EndGame, gameStatus);
                        }
                        processId = 0;
                    }
                }
                if (processId == 0)
                {
                    // dead
                    filepathsToDelete.Add(fileInfo.FullName);
                }
                else
                {
                    if (!_statusMap.ContainsKey(processId))
                    {
                        // new game found
                        var gameStatus = new GameStatus();
                        gameStatus.ProcessId = processId;
                        string heartbeatFile = MagFilter.FileLocations.GetRunningProcessDllToExeFilepath(processId);
                        gameStatus.ProcessStatusFilepath = heartbeatFile;
                        _statusMap.Add(processId, gameStatus);
                        // Do not notify yet because we don't have the gameStatus populated yet
                    }
                }
            }
            foreach (string filepath in filepathsToDelete)
            {
                File.Delete(filepath);
            }
            _lastCleanupUtc = DateTime.UtcNow;
        }
        private void NotifyGameChange(GameChangeType changeType, GameStatus gameStatus)
        {
            if (GameChangeEvent != null)
            {
                GameChangeEvent(changeType, gameStatus);
            }
        }
    }
}
