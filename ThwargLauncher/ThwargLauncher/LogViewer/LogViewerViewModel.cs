using System;
using System.Collections.ObjectModel;

namespace ThwargLauncher
{
    class LogViewerViewModel
    {
        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public LogViewerViewModel()
        {
            LogEntries = new ObservableCollection<LogEntry>();
            Logger.Instance.MessageEvent += Logger_MessageEvent;
        }

        void Logger_MessageEvent(Logger.LogLevel level, string msg)
        {
            LogEntry logmsg = new LogEntry(msg);
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(logmsg)));
        }
        public void LogViewer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logger.Instance.MessageEvent -= Logger_MessageEvent;
        }

    }
}
