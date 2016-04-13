using System;
using System.Collections.Generic;
using System.Text;

namespace MagFilter
{
    class Heartbeat
    {
        private static Heartbeat theHeartbeat = new Heartbeat();

        public static void LaunchHeartbeat()
        {
            theHeartbeat.StartBeating();
        }
        private System.Windows.Forms.Timer _timer = null;
        private string _filepath;
        private void StartBeating()
        {
            string filename = string.Format("game_{0}.txt", System.Diagnostics.Process.GetCurrentProcess().Id);
            _filepath = System.IO.Path.Combine(FileLocations.GetRunningFolder(), filename);

            int intervalMilliseconds = 3000;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = intervalMilliseconds;
            _timer.Tick += _timer_Tick;
            _timer.Enabled = true;
            _timer.Start();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            using (var file = new System.IO.StreamWriter(_filepath, append: false))
            {
                TimeSpan span = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
                file.WriteLine("UptimeSeconds:{0}", (int)span.TotalSeconds);
            }
        }
    }
}
