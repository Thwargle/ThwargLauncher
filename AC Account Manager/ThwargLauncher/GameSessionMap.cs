using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    class GameSessionMap
    {
        private static object _locker = new object();
        // Member data
        private Dictionary<int, GameSession> _sessionByProcessId = new Dictionary<int, GameSession>();
        private Dictionary<string, GameSession> _sessionByServerAccount = new Dictionary<string, GameSession>();

        public void AddGameSession(GameSession gameSession)
        {
            lock (_locker)
            {
                if (_sessionByProcessId.ContainsKey(gameSession.ProcessId))
                {
                    throw new Exception("Duplicate process id in AddGameSession");
                }
                _sessionByProcessId.Add(gameSession.ProcessId, gameSession);
                string key = GetServerAccountKey(gameSession);
                if (_sessionByServerAccount.ContainsKey(key))
                {
                    throw new Exception("Duplicate server/account in AddGameSession");
                }
                _sessionByServerAccount.Add(key, gameSession);
            }
        }
        public bool HasGameSessionByProcessId(int processId)
        {
            return GetGameSessionByProcessId(processId) != null;
        }
        public GameSession GetGameSessionByProcessId(int processId)
        {
            lock (_locker)
            {
                if (_sessionByProcessId.ContainsKey(processId))
                {
                    return _sessionByProcessId[processId];
                }
                else
                {
                    return null;
                }
            }
        }
        public ServerAccountStatus GetGameSessionStateByServerAccount(string serverName, string accountName)
        {
            var gameSession = GetGameSessionByServerAccount(serverName, accountName);
            if (gameSession == null)
            {
                return ServerAccountStatus.None;
            }
            else
            {
                return gameSession.Status;
            }
        }
        public GameSession GetGameSessionByServerAccount(string serverName, string accountName)
        {
            lock (_locker)
            {
                string key = GetServerAccountKey(serverName, accountName);
                if (_sessionByServerAccount.ContainsKey(key))
                {
                    return _sessionByServerAccount[key];
                }
                else
                {
                    return null;
                }
            }
        }
        public List<GameSession> GetAllGameSessions()
        {
            var allStatuses = new List<GameSession>();
            foreach (var gameSession in _sessionByProcessId.Values)
            {
                allStatuses.Add(gameSession);
            }
            return allStatuses;
        }
        public void RemoveGameSessionByProcessId(int processId)
        {
            lock (_locker)
            {
                if (_sessionByProcessId.ContainsKey(processId))
                {
                    GameSession gameSession = _sessionByProcessId[processId];
                    _sessionByProcessId.Remove(processId);
                    _sessionByServerAccount.Remove(GetServerAccountKey(gameSession));
                }
            }
        }
        private string GetServerAccountKey(GameSession gameSession)
        {
            return GetServerAccountKey(gameSession.ServerName, gameSession.AccountName);
        }
        private string GetServerAccountKey(string serverName, string accountName)
        {
            string key = string.Format("{0}:{1}", serverName, accountName);
            return key;
        }
    }
}
