using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagFilter
{
    class log
    {
        private static object _locker = new object();
        private static string _logFilepath = "";
        private enum LOG_LEVEL { Min=0, None=0, Error=1, Info=2, Debug=3, Max=3 };
        private static LOG_LEVEL _logLevel = 0;

        static log()
        {
            InitConfiguration();
        }
        public static string GetLogFilepath()
        {
            return _logFilepath;
        }
        public static void WriteError(string msg) { WriteMsg(LOG_LEVEL.Error, msg); }
        public static void WriteInfo(string msg) { WriteMsg(LOG_LEVEL.Info, msg); }
        public static void WriteDebug(string msg) { WriteMsg(LOG_LEVEL.Debug, msg); }
        /// <summary>
        /// This is the main logging code
        ///// </summary>
        private static void WriteMsg(LOG_LEVEL level, string msg)
        {
            if (msg == "REINITIALIZE") { InitConfiguration(); return; }
            if (_logLevel < level) { return; }
            lock (_locker)
            {
                using (StreamWriter file = new StreamWriter(_logFilepath, append: true))
                {
                    file.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss} {1,-6} {2}", DateTime.Now, level.ToString(), msg));
                }
            }
        }
        /// <summary>
        /// Initialize log settings
        /// Called from static class constructor
        /// </summary>
        private static void InitConfiguration()
        {
            // Get config settings from dll config
            // This is not provided OOB with .NET, so this AssemblySettings class performs this task
            AssemblySettings settings = new AssemblySettings();
            InitConfigureLogLocation(settings);
            InitConfigureLogLevel(settings);
        }
        /// <summary>
        /// Initialize log location
        /// Called from static class constructor
        /// </summary>
        private static void InitConfigureLogLocation(AssemblySettings settings)
        {
            string filepath = settings["LogFilepath"];

            // If no log path specified, default to our normal app logs folder
            if (string.IsNullOrEmpty(filepath))
            {
                filepath = FileLocations.AppLogsFolder + @"\Mag-Filter_%PID%_log.txt";
            }
            _logFilepath = FileLocations.ExpandFilepath(filepath);
            // Create any needed folders
            FileLocations.CreateAnyNeededFoldersOfFile(_logFilepath);
        }
        /// <summary>
        /// Initialize log verbosity
        /// Called from static class constructor
        /// </summary>
        private static void InitConfigureLogLevel(AssemblySettings settings)
        {
            _logLevel = LOG_LEVEL.None;
            string str = settings["LogLevel"];
            _logLevel = ReadLogLevelFromString(str, LOG_LEVEL.None);
        }
        private static LOG_LEVEL ReadLogLevelFromString(string text, LOG_LEVEL defval)
        {
            if (string.IsNullOrEmpty(text)) { return defval; }
            if (EqStr(text, "None")) { return LOG_LEVEL.None; }
            if (EqStr(text, "Error")) { return LOG_LEVEL.Error; }
            if (EqStr(text, "Info")) { return LOG_LEVEL.Info; }
            if (EqStr(text, "Debug")) { return LOG_LEVEL.Debug; }
            int value = -9;
            if (!int.TryParse(text, out value)) { return defval; }
            if (value < (int)LOG_LEVEL.Min || value > (int)LOG_LEVEL.Max) { return defval; }
            return (LOG_LEVEL)value;
        }
        private static bool EqStr(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            {
                return string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2);
            }
            return (string.Compare(str1, str2, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}
