using System;
using System.Collections.Generic;
using System.Text;

namespace MagFilter.Channels
{
    class ChannelWriter
    {
        public void WriteCommandsToFile(Channel channel)
        {
            string filepath = GetChannelOutboundFilepath(channel);
            var writer = new CommandWriter();
            var cmdset = new CommandSet(channel.OutboundCommands, channel.LastInboundProcessed);
            writer.WriteCommandsToFile(cmdset, filepath);
        }
        public void ReadCommandsFromFile(Channel channel)
        {
            string filepath = GetChannelInboundFilepath(channel);
            var writer = new CommandWriter();
            var cmdset = writer.ReadCommandsFromFile(filepath);
            if (cmdset != null)
            {
                // process their acknowledgement
                channel.ProcessAcknowledgement(cmdset.Acknowledgement);
                // Append any new commands, and figure out our new acknowledgement value
                DateTime latest = channel.LastInboundProcessed;
                foreach (var cmd in cmdset.Commands)
                {
                    if (cmd.TimeStamp > channel.LastInboundProcessed)
                    {
                        if (cmd.TimeStamp > latest)
                        {
                            latest = cmd.TimeStamp;
                        }
                        channel.InboundCommands.Add(cmd);
                    }
                }
                channel.LastInboundProcessed = latest;
            }
        }
        private string GetChannelOutboundFilepath(Channel channel)
        {
            string prefix = (channel.InGameDll ? "outcmds" : "incmds");
            string filename = string.Format("{0}_{1}", prefix, channel.ProcessId);
            string filepath = System.IO.Path.Combine(FileLocations.GetRunningFolder(), filename);
            return filepath;
        }
        private string GetChannelInboundFilepath(Channel channel)
        {
            string prefix = (!channel.InGameDll ? "outcmds" : "incmds");
            string filename = string.Format("{0}_{1}", prefix, channel.ProcessId);
            string filepath = System.IO.Path.Combine(FileLocations.GetRunningFolder(), filename);
            return filepath;
        }
    }
}
