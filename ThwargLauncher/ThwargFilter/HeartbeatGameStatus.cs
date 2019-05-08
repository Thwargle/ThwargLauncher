using System;

namespace ThwargFilter
{
    public class HeartbeatGameStatus
    {
        public const string MASTER_FILE_VERSION = "1.4";
        public const string MASTER_FILE_VERSION_COMPAT = "1";

        public string FileVersion;
        public string ServerName;
        public string AccountName;
        public string CharacterName;
        public int UptimeSeconds;
        public int ProcessId;
        public string TeamList; // separated by commas and no spaces
        public string ThwargFilterVersion;
        public string ThwargFilterFilePath;
        public bool IsOnline;
        public int LastServerDispatchSecondsAgo;
    }
}
