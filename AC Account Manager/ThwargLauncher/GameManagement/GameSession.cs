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
        public string ProcessStatusFilepath;
        public int UptimeSeconds = -1;
        public ServerAccountStatus Status = ServerAccountStatus.None;
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
        
        // Implementation
        private string _teamList = ""; // last assigned
        private HashSet<string> _teamSet = new HashSet<string>();
        private static HashSet<string> ParseTeamSetFromString(string teamstring)
        {
            var teamset = new HashSet<string>();
            string[] teamNames = teamstring.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string teamName in teamNames)
            {
                teamset.Add(teamName);
            }
            return teamset;
        }
    }
}
