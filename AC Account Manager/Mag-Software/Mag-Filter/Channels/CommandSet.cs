using System;
using System.Collections.Generic;

namespace MagFilter.Channels
{
    class CommandSet
    {
        public readonly List<Command> Commands;
        public readonly DateTime Acknowledgement;
        public CommandSet(List<Command> commands, DateTime acknowledgement)
        {
            this.Commands = commands;
            this.Acknowledgement = acknowledgement;
        }
    }
}
