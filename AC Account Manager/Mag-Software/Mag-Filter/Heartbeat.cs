using System;
using System.Collections.Generic;
using System.Text;

namespace MagFilter
{
    class Heartbeat
    {
        private static object _locker = new object();
        private static Heartbeat theHeartbeat = new Heartbeat();

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
        public static void LaunchHeartbeat()
        {
            theHeartbeat.StartBeating();
        }
        private System.Windows.Forms.Timer _timer = null;
        private string _gameToLauncherFilepath;
        private void StartBeating()
        {
            int dllProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            _gameToLauncherFilepath = FileLocations.GetRunningProcessDllToExeFilepath(dllProcessId);

            int intervalMilliseconds = 3000;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = intervalMilliseconds;
            _timer.Tick += timer_Tick;
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
                    log.WriteLogMsg("process exit");
                    _timer.Stop();
                }
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            SendMessageImpl(null, null);
        }
        public static void SendImmediateMessage(string key, string value)
        {
            lock (_locker)
            {
                theHeartbeat._timer.Stop();
                theHeartbeat.SendMessageImpl(key, value);
                theHeartbeat._timer.Start();
            }
        }
        /// <summary>
        /// This may be called on timer thread *OR* on external caller's thread
        /// </summary>
        private void SendMessageImpl(string key, string value)
        {
            try
            {
                LaunchControl.RecordHeartbeatStatus(_gameToLauncherFilepath, _status, EncodeString(key), EncodeString(value));
            }
            catch
            {
                log.WriteLogMsg("Exception writing heartbeat status");
            }
    }
        private static string EncodeString(string text)
        {
            return LaunchControl.EncodeString(text);
        }
    }
}
