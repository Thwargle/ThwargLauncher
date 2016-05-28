using System;
using System.Text;

namespace ThwargLauncher
{
    /// <summary>
    /// Publish log message to any subscribers
    /// </summary>
    public class Logger
    {
        public static void BeginLogging(string msg)
        {
            Instance.SendMessage(LogLevel.Begin, msg);
        }
        public static void WriteError(string text)
        {
            Instance.SendMessage(LogLevel.Error, text);
        }
        public static void WriteInfo(string text)
        {
            Instance.SendMessage(LogLevel.Info, text);
        }

        public enum LogLevel { Begin, Error, Info };
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
