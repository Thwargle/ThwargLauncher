using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    class LogViewerViewModel
    {
        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public LogViewerViewModel()
        {
            LogEntries = new ObservableCollection<LogEntry>();
        }
    }
}
