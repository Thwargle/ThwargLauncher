using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagFilter
{
    class log
    {
        private static object locker = new object();
        private static string logpath = FileLocations.PluginPersonalFolder.FullName + @"\MagFilter2_Log.txt";

        public static void WriteLogMsg(string logText)
        {
            lock (locker)
            {
                // Write the string to a file.

                using (StreamWriter file = new StreamWriter(logpath, append: true))
                {
                    file.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss} {1}", DateTime.Now, logText));
                }
            }
        }
    }
}
