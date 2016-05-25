using System;

namespace MagFilter.Channels
{
    public class Command
    {
        public readonly DateTime TimeStamp;
        public readonly string CommandString;
        public Command(DateTime timeStamp, string commandString)
        {
            this.TimeStamp = timeStamp;
            this.CommandString = commandString;
        }
        public override string ToString()
        {
            return string.Format("{0:S}: {1}", TimeStamp, CommandString);
        }
    }
}
