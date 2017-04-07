using System;
using System.Collections.Generic;

namespace ThwargLauncher
{
    class GameSession
    {
        public string ServerName;
        public string AccountName;
        public string CharacterName;
        public int ProcessId;
        public string ProcessIdKey;
        public string ProcessStatusFilepath;
        public int UptimeSeconds = -1;
        public ServerAccountStatusEnum Status = ServerAccountStatusEnum.None;
        public DateTime LastGoodStatusUtc = DateTime.MinValue;
        public MagFilter.Channels.Channel GameChannel;
        public void AssignTeamSetFromString(string teamlist)
        {
            if (teamlist == _teamList) { return; }
            _teamList = teamlist;
            _teamSet = ParseTeamSetFromString(_teamList);
        }
        public int TeamCount { get { return _teamSet.Count; } }
        public string TeamList { get { return _teamList; } }
        public bool HasAnyTeam(List<string> teams)
        {
            foreach (var teamName in teams)
            {
                if (_teamSet.Contains(teamName))
                {
                    return true;
                }
            }
            return false;
        }
        public void StopSessionWatcher()
        {
            var writer = new MagFilter.Channels.ChannelWriter();
            writer.StopWatcher(this.GameChannel);
        }
        
        // Implementation
        private string _teamList = ""; // last assigned
        private HashSet<string> _teamSet = new HashSet<string>();
        private static HashSet<string> ParseTeamSetFromString(string teamstring)
        {
            var teamset = new HashSet<string>();
            if (!string.IsNullOrEmpty(teamstring))
            {
                string[] teamNames = teamstring.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string teamName in teamNames)
                {
                    teamset.Add(teamName);
                }
            }
            return teamset;
        }
    }
}
