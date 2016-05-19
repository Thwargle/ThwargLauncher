using System;
using System.IO;
using System.Text;

namespace ThwargLauncher
{
    public static class Log
    {
        private static object _locker = new object();

        public static void WriteError(string logText)
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
                    file.WriteLine(logText);
                }
            }
        }
        internal static string GetLauncherLogPath()
        {
            string filepath = WriteableDataFolder.FullName + @"\ThwargLauncher_log.txt";
            return filepath;
        }
        internal static DirectoryInfo WriteableDataFolder
        {
            get
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\ThwargLauncher";
                DirectoryInfo mydirectory = new DirectoryInfo(folder);

                try
                {
                    if (!mydirectory.Exists)
                        mydirectory.Create();
                }
                catch
                {
                }

                return mydirectory;
            }
        }

    }
}
