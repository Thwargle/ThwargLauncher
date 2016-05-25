using System;
using System.Collections.Generic;
using System.Text;

namespace MagFilter.Channels
{
    class Channel
    {
        public static Channel MakeLauncherChannel(int processId)
        {
            return new Channel(dll: false, processId: processId);
        }
        public static Channel MakeGameChannel()
        {
            int myProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
            return new Channel(dll: true, processId: myProcessId);
        }
        private Channel(bool dll, int processId)
        {
            this.InGameDll = dll;
            this.ProcessId = processId;
        }
        public readonly bool InGameDll;
        public readonly int ProcessId;
        public List<Command> OutboundCommands = new List<Command>();
        public List<Command> InboundCommands = new List<Command>();
        public DateTime LastInboundProcessed = DateTime.MinValue;

        public void ProcessAcknowledgement(DateTime ackTime)
        {
            var pending = new List<Command>(OutboundCommands.Count);
            foreach (var cmd in OutboundCommands)
            {
                if (cmd.TimeStamp > ackTime)
                {
                    pending.Add(cmd);
                }
            }
            OutboundCommands = pending;
        }
    }
}
