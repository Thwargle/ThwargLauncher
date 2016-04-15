using System;
using System.Collections.Generic;
using System.Text;

namespace MagFilter
{
    class Heartbeat
    {
        private static object _locker = new object();
        private static Heartbeat theHeartbeat = new Heartbeat();

        public static void RecordServer(string ServerName)
        {
            theHeartbeat._serverName = ServerName;
        }
        public static void RecordAccount(string AccountName)
        {
            theHeartbeat._accountName = AccountName;
        }
        public static void RecordCharacterName(string CharacterName)
        {
            theHeartbeat._characterName = CharacterName;
        }
        public static void LaunchHeartbeat()
        {
            theHeartbeat.StartBeating();
        }
        private System.Windows.Forms.Timer _timer = null;
        private string _serverName;
        private string _accountName;
        private string _characterName;
        private string _gameToLauncherFilepath;
        private void StartBeating()
        {
            int dllProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            _gameToLauncherFilepath = FileLocations.GetRunningProcessDllToExeFilepath(dllProcessId);

            int intervalMilliseconds = 3000;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = intervalMilliseconds;
            _timer.Tick += _timer_Tick;
            _timer.Enabled = true;
            _timer.Start();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            lock (_locker)
            {
                if (_timer != null)
                {
                    log.WriteLogMsg("process exist");
                    _timer.Stop();
                }
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            lock (_locker)
            {
                using (var file = new System.IO.StreamWriter(_gameToLauncherFilepath, append: false))
                {
                    TimeSpan span = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                    file.WriteLine("UptimeSeconds:{0}", (int)span.TotalSeconds);
                    file.WriteLine("ServerName:{0}", _serverName);
                    file.WriteLine("AccountName:{0}", _accountName);
                    file.WriteLine("CharacterName:{0}", _characterName);
                    file.WriteLine("logFilepath:{0}", log.GetLogFilepath());
                }
            }
        }
    }
}
