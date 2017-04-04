using System;
using System.Collections.Generic;
using System.Text;

using Mag.Shared;

namespace MagFilter
{
    class Heartbeat
    {
        private static object _locker = new object();
        private static Heartbeat theHeartbeat = new Heartbeat();
        private Channels.Channel _myChannel = Channels.Channel.MakeGameChannel();
        private MagFilterCommandParser _cmdParser = null;
        private DateTime LastSendAndReceive;
        private const int TIMER_SECONDS = 3;
        private const int TIMER_SKIPSEC = 1; // Skip timer if send & received this recent

        private HeartbeatGameStatus _status = new HeartbeatGameStatus();

        public static void RecordServer(string ServerName)
        {
            theHeartbeat._status.ServerName = ServerName;
        }
        public static void RecordAccount(string AccountName)
        {
            theHeartbeat._status.AccountName = AccountName;
        }
        public static void RecordCharacterName(string CharacterName)
        {
            theHeartbeat._status.CharacterName = CharacterName;
        }
        public static void SendCommand(string commandString)
        {
            theHeartbeat._myChannel.EnqueueOutbound(
                new Channels.Command(DateTime.UtcNow, commandString)
                );
        }
        public static void SetCommandParser(MagFilterCommandParser parser) { theHeartbeat._cmdParser = parser; }
        public static void LaunchHeartbeat()
        {
            theHeartbeat.StartBeating();
        }
        private System.Windows.Forms.Timer _timer = null;
        private string _gameToLauncherFilepath;
        private void StartBeating()
        {
            int dllProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            _gameToLauncherFilepath = FileLocations.GetGameHeartbeatFilepath(dllProcessId);

            int intervalMilliseconds = 1000 * TIMER_SECONDS;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = intervalMilliseconds;
            _timer.Tick += timer_Tick;
            _timer.Enabled = true;
            _timer.Start();
            StartChannelFileWatcher();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }
        private void StartChannelFileWatcher()
        {
            var writer = new Channels.ChannelWriter();
            if (!writer.IsWatcherEnabled(_myChannel))
            {
                writer.StartWatcher(_myChannel);
                _myChannel.FileWatcher.Changed += OnChannelFileWatcherChanged;
            }
        }
        void OnChannelFileWatcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            log.WriteLogMsg("WQW Channel File Watcher Fired");
            Heartbeat.SendAndReceiveImmediately();
        }
        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            lock (_locker)
            {
                if (_myChannel != null)
                {
                    var writer = new Channels.ChannelWriter();
                    if (writer.IsWatcherEnabled(_myChannel))
                    {
                        writer.StopWatcher(_myChannel);
                    }
                }
                if (_timer != null)
                {
                    _timer.Stop();
                }
                log.WriteLogMsg("process exit");
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (System.Threading.Monitor.TryEnter(_locker))
            {
                if ((DateTime.UtcNow - LastSendAndReceive).TotalMilliseconds < 1000 * TIMER_SKIPSEC)
                {
                    return;
                }
                try
                {
                    SendAndReceiveCommands();
                }
                finally
                {
                    System.Threading.Monitor.Exit(_locker);
                }
            }
        }
        public static void SendAndReceiveImmediately()
        {
            if (System.Threading.Monitor.TryEnter(_locker))
            {
                try
                {
                    theHeartbeat._timer.Stop();
                    theHeartbeat.SendAndReceiveCommands();
                    theHeartbeat._timer.Start();
                }
                finally
                {
                    System.Threading.Monitor.Exit(_locker);
                }
            }
        }
        /// <summary>
        /// This may be called on timer thread *OR* on external caller's thread
        /// </summary>
        private void SendAndReceiveCommands()
        {
            bool success = true;
            try
            {
                _status.TeamList = _cmdParser.GetTeamList();
                LaunchControl.RecordHeartbeatStatus(_gameToLauncherFilepath, _status);
            }
            catch (Exception exc)
            {
                success = false;
                log.WriteLogMsg("Exception writing heartbeat status: " + exc.ToString());
            }
            try
            {
                if (_myChannel.NeedsToWrite)
                {
                    var writer = new Channels.ChannelWriter();
                    writer.WriteCommandsToFile(_myChannel);
                }
            }
            catch (Exception exc)
            {
                success = false;
                log.WriteLogMsg("Exception writing command file status: " + exc.ToString());
            }
            try
            {
                ReadAndProcessInboundCommands();
            }
            catch (Exception exc)
            {
                success = false;
                log.WriteLogMsg("Exception reading command file status: " + exc.ToString());
            }
            if (success)
            {
                LastSendAndReceive = DateTime.UtcNow;
            }
        }
        private void ReadAndProcessInboundCommands()
        {
            var writer = new Channels.ChannelWriter();
            writer.ReadCommandsFromFile(_myChannel);
            while (_myChannel.HasInboundCommandCount())
            {
                var cmd = _myChannel.DequeueInbound();
                ExecuteGameCommandString(cmd.CommandString);
                if (cmd.TimeStampUtc > _myChannel.LastInboundProcessedUtc)
                {
                    _myChannel.LastInboundProcessedUtc = cmd.TimeStampUtc;
                    _myChannel.NeedsToWrite = true;
                }
            }
        }
        private void ExecuteGameCommandString(string commandString)
        {
            // Note: "Mag.Shared.PostMessageTools.SendMsg" does not work
            // DecalProxy.DispatchChatToBoxWithPluginIntercept(commandString) works
            _cmdParser.ExecuteCommandFromLauncher(commandString); // cross-thread
        }
        private static string EncodeString(string text)
        {
            return LaunchControl.EncodeString(text);
        }
    }
}
