using System;
using System.Text;

namespace ThwargLauncher
{
    public static class Logger
    {
        private static object _locker = new object();

        public static void BeginLogging()
        {
            LogWriter.WriteHeader();
        }
        internal static string GetLauncherLogPath() { return LogWriter.GetLauncherLogPath(); }
        public static void WriteError(string text)
        {
            LogWriter.WriteError(text);
        }
        public static void WriteInfo(string text)
        {
            LogWriter.WriteInfo(text);
        }
    }
}
