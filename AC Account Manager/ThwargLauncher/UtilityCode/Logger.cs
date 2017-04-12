using System;
using System.Text;

namespace ThwargLauncher
{
    /// <summary>
    /// Publish log message to any subscribers
    /// </summary>
    public class Logger
    {
        public static void WriteError(string text) { Instance.SendMessage(LogLevel.Error, text); }
        public static void WriteError(string fmt, params object[] args) { WriteError(string.Format(fmt, args)); }

        public static void WriteInfo(string text) { Instance.SendMessage(LogLevel.Info, text); }
        public static void WriteInfo(string fmt, params object[] args) { WriteInfo(string.Format(fmt, args)); }
        
        public static void WriteDebug(string text) { Instance.SendMessage(LogLevel.Debug, text); }
        public static void WriteDebug(string fmt, params object[] args) { WriteDebug(string.Format(fmt, args)); }

        public enum LogLevel { Error, Info, Debug };
        public delegate void MsgHandler(LogLevel level, string msg);
        public event MsgHandler MessageEvent;

        private static Logger theInstance = new Logger();

        public static Logger Instance { get { return theInstance; } }

        private void SendMessage(LogLevel level, string msg)
        {
            if (MessageEvent != null)
            {
                MessageEvent(level, msg);
            }
        }
    }
}
