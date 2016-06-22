using System;
using System.IO;
using System.Text;

namespace ThwargLauncher
{
    /// <summary>
    /// A method that subscribes to the central Logger instance, and records log messages to a file
    /// </summary>
    public class LogWriter
    {
        private object _locker = new object();
        private readonly string _filepath;
        public LogWriter(string filepath)
        {
            this._filepath = filepath;
            Initialize();
            WriteHeader();
        }
        public void Initialize()
        {
            Logger.Instance.MessageEvent += Logger_MessageEvent;
        }
        private void Logger_MessageEvent(Logger.LogLevel level, string msg)
        {
            WriteLineImpl(level, msg);
        }
        private void WriteHeader()
        {
            // Write the string to a file.
            lock (_locker)
            {
                using (StreamWriter file = new StreamWriter(_filepath, append: true))
                {
                    file.WriteLine("Time (UTC): {0}", DateTime.UtcNow);
                    var osInfo = new OsUtil.OperatingSystemInfo();
                    file.WriteLine("OS: {0}", osInfo.getOSInfo());
                    file.WriteLine("Culture: {0}", System.Globalization.CultureInfo.CurrentCulture.Name);
                    file.WriteLine("AssemblyVer: {0}", System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
                }
            }
        }
        private void WriteLineImpl(Logger.LogLevel level, string logText)
        {
            if (level == Logger.LogLevel.Error)
            {
                logText = "* " + logText;
            }
            else
            {
                logText = "  " + logText;
            }
            // Write the string to a file.
            lock (_locker)
            {
                using (StreamWriter file = new StreamWriter(_filepath, append: true))
                {
                    file.WriteLine("{0:yyyy-MM-dd HH:mm:ss}: {1}", DateTime.UtcNow, logText);
                }
            }
        }
    }
}
