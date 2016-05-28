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
            // TODO
            // For now, just broadcast the command back out to all active games
            foreach (var gameSession in _gameSessionMap.GetAllGameSessions())
            {
                if (gameSession.GameChannel != null)
                {
                    Logger.WriteInfo(string.Format(
                        "Sending command '{0}' to server '{1}' and account '{2}'",
                        command, gameSession.ServerName, gameSession.AccountName
                        ));
                    var magCmd = new MagFilter.Channels.Command(DateTime.UtcNow, command);
                    gameSession.GameChannel.EnqueueOutbound(magCmd);
                }
            }
        }
    }
}
