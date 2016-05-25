using System;

namespace MagFilter.Channels
{
    public class Command
    {
        public readonly DateTime TimeStampUtc;
        public readonly string CommandString;
        public Command(DateTime timeStampUtc, string commandString)
        {
            this.TimeStampUtc = timeStampUtc;
            this.CommandString = commandString;
        }
        public override string ToString()
        {
            return string.Format("{0:S}: {1}", TimeStampUtc, CommandString);
        }
    }
}
