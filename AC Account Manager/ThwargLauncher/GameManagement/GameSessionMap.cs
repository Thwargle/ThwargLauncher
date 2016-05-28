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
                    // Note: This can happen at startup
                    Logger.WriteError(string.Format("Duplicate process id in AddGameSession: {0}", gameSession.ProcessId));
                }
                else
                {
                    _sessionByProcessId.Add(gameSession.ProcessId, gameSession);
                }
                string key = GetServerAccountKey(gameSession);
                if (_sessionByServerAccount.ContainsKey(key))
                {
                    Logger.WriteError(string.Format("Duplicate server/account in AddGameSession: {0}", key));
                }
                else
                {
                    _sessionByServerAccount.Add(key, gameSession);
                }
            }
        }
        public void SetGameSessionProcessId(GameSession gameSession, int processId)
        {
            lock (_locker)
            {
                if (gameSession.ProcessId == processId) { return; }
                // This should only occur with starting game sessions
                // when they find their process id after finding their heartbeat file
                if (gameSession.ProcessId != 0)
                {
                    // This should not occur
                    // ProcessId should only change when a starting game session with process id 0 (unknown)
                    // finds its heartbeat file the first time
                }
                _sessionByProcessId.Remove(gameSession.ProcessId);
                gameSession.ProcessId = processId;
                _sessionByProcessId.Add(gameSession.ProcessId, gameSession);
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
                return GetGameSessionByServerAccountImplUnlocked(serverName, accountName);
            }
        }
        private GameSession GetGameSessionByServerAccountImplUnlocked(string serverName, string accountName)
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
        public List<GameSession> GetAllGameSessions()
        {
            var allStatuses = new List<GameSession>();
            lock (_locker)
            {
                foreach (var gameSession in _sessionByProcessId.Values)
                {
                    allStatuses.Add(gameSession);
                }
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
        public void StartLaunchingSession(string serverName, string accountName)
        {
            lock (_locker)
            {
                var gameSession = GetGameSessionByServerAccountImplUnlocked(serverName, accountName);
                if (gameSession != null)
                {
                    gameSession.Status = ServerAccountStatus.Starting;
                }
                else
                {
                    gameSession = new GameSession();
                    gameSession.ServerName = serverName;
                    gameSession.AccountName = accountName;
                    gameSession.Status = ServerAccountStatus.Starting;
                    AddGameSession(gameSession);
                }
            }
        }
        public void EndLaunchingSession(string serverName, string accountName)
        {
            lock (_locker)
            {
                var gameSession = GetGameSessionByServerAccountImplUnlocked(serverName, accountName);
                if (gameSession != null)
                {
                    if (gameSession.Status == ServerAccountStatus.Starting)
                    {
                        // If it never made it out of starting, then it should be set to warning
                        gameSession.Status = ServerAccountStatus.Warning;
                    }
                }
            }
        }
        public void EndAllLaunchingSessions()
        {
            lock (_locker)
            {
                foreach (var gameSession in _sessionByProcessId.Values)
                {
                    if (gameSession.Status == ServerAccountStatus.Starting)
                    {
                        gameSession.Status = ServerAccountStatus.Warning;
                    }
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
