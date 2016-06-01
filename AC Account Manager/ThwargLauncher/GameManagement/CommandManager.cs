using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
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
            _gameMonitor.GameCommandEvent += _gameMonitor_GameCommandEvent;
            // TODO - should shut this down
        }
        public void StopHandling()
        {
            // TODO - ApplicationCoordinator should set up something to call this
            _gameMonitor.GameCommandEvent -= _gameMonitor_GameCommandEvent;
        }
        void _gameMonitor_GameCommandEvent(GameSession inboundGameSession, string command)
        {
            Logger.WriteInfo(string.Format(
                "Command received from server='{0}', account='{1}': {2}",
                inboundGameSession.ServerName, inboundGameSession.AccountName, command));
            string commandString = "";
            // TODO - implement & handle team filtered commands
            if (IsCommandPrefix(command, "broadcast ", ref commandString))
            {
                if (!string.IsNullOrWhiteSpace(commandString))
                {
                    foreach (var gameSession in _gameSessionMap.GetAllGameSessions())
                    {
                        if (gameSession.GameChannel != null)
                        {
                            Logger.WriteInfo(string.Format(
                                "Sending command '{0}' to server '{1}' and account '{2}'",
                                commandString, gameSession.ServerName, gameSession.AccountName
                                                 ));
                            var magCmd = new MagFilter.Channels.Command(DateTime.UtcNow, commandString);
                            gameSession.GameChannel.EnqueueOutbound(magCmd);
                        }
                    }
                }
            }
            else if (IsCommandPrefix(command, "createteam ", ref commandString))
            {
                var members = ParseTokens(commandString);
                foreach (string member in members)
                {
                    // TODO check for character with this name
                }
                Logger.WriteInfo("Received createteam command; not yet implemented - TODO");
            }
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
