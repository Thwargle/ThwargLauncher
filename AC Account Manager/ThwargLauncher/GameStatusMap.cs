using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    class GameStatusMap
    {
        private static object _locker = new object();
        // Member data
        private Dictionary<int, GameStatus> _statusByProcessId = new Dictionary<int, GameStatus>();
        private Dictionary<string, GameStatus> _statusByServerAccount = new Dictionary<string, GameStatus>();

        public void AddGameStatus(GameStatus gameStatus)
        {
            lock (_locker)
            {
                if (_statusByProcessId.ContainsKey(gameStatus.ProcessId))
                {
                    throw new Exception("Duplicate process id in AddGameStatus");
                }
                _statusByProcessId.Add(gameStatus.ProcessId, gameStatus);
                string key = GetServerAccountKey(gameStatus);
                if (_statusByServerAccount.ContainsKey(key))
                {
                    throw new Exception("Duplicate server/account in AddGameStatus");
                }
                _statusByServerAccount.Add(key, gameStatus);
            }
        }
        public bool HasGameStatusByProcessId(int processId)
        {
            return GetGameStatusByProcessId(processId) != null;
        }
        public GameStatus GetGameStatusByProcessId(int processId)
        {
            lock (_locker)
            {
                if (_statusByProcessId.ContainsKey(processId))
                {
                    return _statusByProcessId[processId];
                }
                else
                {
                    return null;
                }
            }
        }
        public bool HasGameStatusByServerAccount(string serverName, string accountName)
        {
            return GetGameStatusByServerAccount(serverName, accountName) != null;
        }
        public GameStatus GetGameStatusByServerAccount(string serverName, string accountName)
        {
            lock (_locker)
            {
                string key = GetServerAccountKey(serverName, accountName);
                if (_statusByServerAccount.ContainsKey(key))
                {
                    return _statusByServerAccount[key];
                }
                else
                {
                    return null;
                }
            }
        }
        public List<GameStatus> GetAllGameStatuses()
        {
            var allStatuses = new List<GameStatus>();
            foreach (var gameStatus in _statusByProcessId.Values)
            {
                allStatuses.Add(gameStatus);
            }
            return allStatuses;
        }
        public void RemoveGameStatusByProcessId(int processId)
        {
            lock (_locker)
            {
                if (_statusByProcessId.ContainsKey(processId))
                {
                    GameStatus gameStatus = _statusByProcessId[processId];
                    _statusByProcessId.Remove(processId);
                    _statusByServerAccount.Remove(GetServerAccountKey(gameStatus));
                }
            }
        }
        private string GetServerAccountKey(GameStatus gameStatus)
        {
            return GetServerAccountKey(gameStatus.ServerName, gameStatus.AccountName);
        }
        private string GetServerAccountKey(string serverName, string accountName)
        {
            string key = string.Format("{0}:{1}", serverName, accountName);
            return key;
        }
    }
}
