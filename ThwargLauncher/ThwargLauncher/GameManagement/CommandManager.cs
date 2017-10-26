using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    /// <summary>
    /// The command manager listens for commands from running games and handles them
    /// Which means it also talks to running games, e.g., to broadcast commands
    /// It relies on the gameMonitor to communicate with games
    /// </summary>
    class CommandManager
    {
        private GameMonitor _gameMonitor;
        private GameSessionMap _gameSessionMap;

        public CommandManager(GameMonitor gameMonitor, GameSessionMap gameSessionMap)
        {
            if (gameMonitor == null) { throw new Exception("Null GameMonitor in CommandManager()"); }
            if (gameSessionMap == null) { throw new Exception("Null GameSessionMap in CommandManager()"); }

            _gameMonitor = gameMonitor;
            _gameSessionMap = gameSessionMap;

            StartHandling();
        }
        private void StartHandling()
        {
            _gameMonitor.GameCommandEvent += HandleGameCommandEvent;
            // TODO - should shut this down
        }
        public void StopHandling()
        {
            // TODO - ApplicationCoordinator should set up something to call this
            _gameMonitor.GameCommandEvent -= HandleGameCommandEvent;
        }
        void HandleGameCommandEvent(GameSession inboundGameSession, string command)
        {
            Logger.WriteInfo(string.Format(
                "Command received from {0}: {1}",
                inboundGameSession.Description, command));
            string commandString = "";
            if (IsCommandPrefix(command, "broadcast ", ref commandString))
            {
                HandleBroadcastCommand(inboundGameSession, commandString);
            }
            else if (IsCommandPrefix(command, "createteam ", ref commandString))
            {
                HandleCreateTeamCommand(inboundGameSession, commandString);
            }
        }
        private class TeamParsedCommand
        {
            private List<string> TeamNames = new List<string>();

        }
        private void HandleBroadcastCommand(GameSession inboundGameSession, string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString)) { return; }

            List<string> teamNames = FindTeamsSpecified(ref commandString);
            if (string.IsNullOrEmpty(commandString)) { return; }
            foreach (var gameSession in _gameSessionMap.GetAllGameSessions())
            {
                if (gameSession.GameChannel == null) { continue; }
                if (teamNames == null || gameSession.HasAnyTeam(teamNames))
                {
                    Logger.WriteInfo(string.Format(
                        "Sending command '{0}' to {1}",
                        commandString, gameSession.Description
                                            ));
                    SendGameCommand(gameSession, commandString);
                }
            }
        }
        private List<string> FindTeamsSpecified(ref string commandString)
        {
            string teamtok = null;
            if (TryProcessArg(ref commandString, "/t:", out teamtok))
            {
                return ParseTeamList(teamtok);
            }
            if (TryProcessArg(ref commandString, "/team:", out teamtok))
            {
                return ParseTeamList(teamtok);
            }
            return null;
        }
        private List<string> ParseTeamList(string teamtok)
        {
            List<string> teamNames = teamtok.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return teamNames;
        }
        private bool TryProcessArg(ref string commandString, string prefix, out string argstr)
        {
            argstr = null;
            if (!commandString.StartsWith(prefix)) { return false; }
            int pos = commandString.IndexOf(" ");
            if (pos == -1 || pos == commandString.Length - 1)
            {
                commandString = null;
                return true;
            }
            argstr = commandString.Substring(prefix.Length, pos - prefix.Length);
            commandString = commandString.Substring(pos + 1);
            if (string.IsNullOrWhiteSpace(commandString)) { argstr = null;  commandString = null; return true; }
            return true;
        }
        private void HandleCreateTeamCommand(GameSession inboundGameSession, string commandString)
        {
            var args = ParseTokens(commandString).Distinct().ToList();
            if (args.Count == 0)
            {
                Logger.WriteError("Ignoring createteam command with no arguments");
                return;
            }
            if (args.Count == 1)
            {
                Logger.WriteError("Ignoring createteam command with one argument: " + args[0]);
                return;
            }
            string teamName = null;
            var sessions = new List<GameSession>();
            foreach (string argstr in args)
            {
                if (teamName == null)
                {
                    teamName = argstr;
                }
                else
                {
                    string charName = argstr;
                    sessions.AddRange(_gameSessionMap.GetGameSessionsByCharacterName(charName));
                }
            }
            foreach (var gameSession in sessions)
            {
                string joincmdstr = string.Format("/mf jointeam {0}", teamName);
                SendGameCommand(gameSession, joincmdstr);
            }
            Logger.WriteInfo(string.Format(
                "Implemented createteam by sending {0} jointeam commands",
                sessions.Count));
        }
        private string quote(string text)
        {
            if (string.IsNullOrEmpty(text)) { return "''"; }
            if (text[0] == '\'' || text[0] == '"')
            {
                return text;
            }
            else
            {
                return '\'' + text + '\'';
            }
        }
        private void SendGameCommand(GameSession gameSession, string cmdtext)
        {
            var magCmd = new MagFilter.Channels.Command(DateTime.UtcNow, cmdtext);
            gameSession.GameChannel.EnqueueOutbound(magCmd);
        }
        public IList<string> ParseTokens(string text)
        {
            var oldPos = 0;
            int pos = -1;
            var items = new List<string>();
            while (true)
            {
                oldPos = pos + 1;
                if (oldPos >= text.Length)
                {
                    break;//last item and without value
                }
                if (text[oldPos] == '"')
                {
                    // jump to before quote
                    oldPos += 1;
                    pos = text.IndexOf('"', oldPos);
                    items.Add(text.Substring(oldPos, pos - oldPos));
                }
                else if (text[oldPos] == '\'')
                {
                    // jump to before quote
                    oldPos += 1;
                    pos = text.IndexOf('\'', oldPos);
                    items.Add(text.Substring(oldPos, pos - oldPos));
                }
                else if (text[oldPos] == ' ')
                {
                    ++pos; // ignore whitespace between tokens
                }
                else
                {
                    pos = text.IndexOf(' ', oldPos);
                    if (pos == -1)
                    {
                        items.Add(text.Substring(oldPos));
                        break;//no more items
                    }

                    items.Add(text.Substring(oldPos, pos - oldPos));
                }
            }
            return items;
        }
        public void TestParse()
        {
            TestParseLine("a b c", "[a], [b], [c]");
            TestParseLine("a bde c", "[a], [bde], [c]");
            TestParseLine("a 'b c'", "[a], [b c]");
            TestParseLine("a b 'c' \"alpha\"", "[a], [b], [c], [alpha]");

        }
        private void TestParseLine(string text, string expected)
        {
            var list = ParseTokens(text);
            string result = "[" + string.Join("], [", list.ToArray()) + "]";
            if (result != expected)
            {
                string msg = string.Format("TestParseLine({0}) != {1}", text, expected);
                throw new Exception(msg);
            }
        }

        private bool IsCommandPrefix(string line, string prefix, ref string command)
        {
            if (line.StartsWith(prefix))
            {
                if (line.Length > prefix.Length)
                {
                    command = line.Substring(prefix.Length, line.Length - prefix.Length);
                }
                else
                {
                    command = "";
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
