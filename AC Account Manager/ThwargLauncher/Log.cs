using System;
using System.IO;
using System.Text;

namespace ThwargLauncher
{
    public static class Log
    {
        private static object _locker = new object();

        public static void WriteHeader()
        {
            // Write the string to a file.
            string filepath = GetLauncherLogPath();

            lock (_locker)
            {
                using (StreamWriter file = new StreamWriter(filepath, append: true))
                {
                    file.WriteLine("Time (UTC): {0}", DateTime.UtcNow);
                    var osInfo = new OsUtil.OperatingSystemInfo();
                    file.WriteLine("OS: {0}", osInfo.getOSInfo());
                    file.WriteLine("Culture: {0}", System.Globalization.CultureInfo.CurrentCulture.Name);
                    file.WriteLine("AssemblyVer: {0}", System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
                }
            }
        }
        public static void WriteError(string text)
        {
            WriteLineImpl("* " + text);
        }
        public static void WriteInfo(string text)
        {
            WriteLineImpl("  " + text);
        }
        private static void WriteLineImpl(string logText)
        {
            // Write the string to a file.
            string filepath = GetLauncherLogPath();

            lock (_locker)
            {
                using (StreamWriter file = new StreamWriter(filepath, append: true))
                {
                    file.WriteLine("{0:yyyy-MM-dd HH:mm:ss}: {1}", DateTime.UtcNow, logText);
                }
            }
        }
        internal static string GetLauncherLogPath()
        {
            string filepath = MagFilter.FileLocations.AppLogsFolder + @"\ThwargLauncher-%PID%_log.txt";
            filepath = MagFilter.FileLocations.ExpandFilepath(filepath);
            MagFilter.FileLocations.CreateAnyNeededFolders(filepath);
            return filepath;
        }
    }
}
