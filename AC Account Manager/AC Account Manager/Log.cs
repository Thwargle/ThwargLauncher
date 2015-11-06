using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AC_Account_Manager
{
    public static class Log
    {
        public static void WriteLog(string logText)
        {
            // Write the string to a file.
            string filepath = GetLogFilePath();

            using (StreamWriter file = new StreamWriter(filepath, append: true))
            {
                file.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss} {1}", DateTime.Now, logText));
            }
        }
        internal static string GetLogFilePath()
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
