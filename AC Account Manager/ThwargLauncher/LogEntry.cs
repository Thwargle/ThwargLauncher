using System;
using System.Collections.Generic;

namespace ThwargLauncher
{
    public class LogEntry : PropertyChangedBase
    {
        public LogEntry(string msg) { this.DateTime = DateTime.Now; this.Message = msg; }
        public LogEntry() { this.Message = ""; }

        public DateTime DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }
}
