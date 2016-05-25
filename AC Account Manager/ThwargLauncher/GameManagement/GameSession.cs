using System;

namespace ThwargLauncher
{
    class GameSession
    {
        public string ServerName;
        public string AccountName;
        public string CharacterName;
        public int ProcessId;
        public string ProcessStatusFilepath;
        public int UptimeSeconds = -1;
        public ServerAccountStatus Status = ServerAccountStatus.None;
        public MagFilter.Channels.Channel GameChannel;
    }
}
