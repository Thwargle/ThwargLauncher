using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    class GameMonitor
    {
        private static object _locker = new object();

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly GameSessionMap _map;
        private Configurator _configurator;
        private TimeSpan _liveInterval; // must be written this recently to be alive
        private TimeSpan _warningInterval; // must be written this recently to be alive
        private DateTime _lastCleanupUtc = DateTime.MinValue;
        private TimeSpan _cleanupInterval = new TimeSpan(0, 5, 0); // 5 minutes
        private DateTime _lastReadProcesFilesUtc = DateTime.MinValue;
        private TimeSpan _rereadProcessFilesInterval = new TimeSpan(0, 1, 0); // 5 minutes
        private bool _rereadRequested = false; // cross-thread access
        private bool _isWorking = false; // reentrancy guard

        public enum GameChangeType { StartGame, EndGame, ChangeGame, ChangeStatus };
        public delegate void GameChangeHandler(GameChangeType changeType, GameSession gameSession);
        public event GameChangeHandler GameChangeEvent;

        public GameMonitor(GameSessionMap map, Configurator configurator)
        {
            _map = map;
            _configurator = configurator;
        }
        public void Start() // main thread
        {
            _liveInterval = TimeSpan.FromSeconds(ConfigSettings.GetConfigInt("HeartbeatFailedTimeoutSeconds", 60));
            _warningInterval = TimeSpan.FromSeconds(ConfigSettings.GetConfigInt("HeartbeatWarningTimeoutSeconds", 15));

            int intervalMilliseconds = 3000;
            //intervalMilliseconds = 20000; // TODO
            _timer.Interval = intervalMilliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }
        public void Stop() // main thread
        {
            _timer.Stop();
        }
        public void QueueReread() // main thread
        {
            lock (_locker)
            {
                if (!_rereadRequested)
                {
                    _rereadRequested = true;
                }
            }
        }
        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isWorking) { return; }
            _isWorking = true;
            if (ShouldWeCleanup())
            {
                CleanupOldProcessFiles();
            }
            if (ShouldWeReadProcessFiles())
            {
                ReadProcessFiles();
            }
            CheckLiveProcessFiles();
            _isWorking = false;
        }
        private bool ShouldWeCleanup()
        {
            bool forceCleanup = false; // for debugging use
            if (forceCleanup || _lastCleanupUtc == DateTime.MinValue)
            {
                return true;
            }
            TimeSpan elapsed = DateTime.UtcNow - _lastCleanupUtc;
            if (elapsed > _cleanupInterval)
            {
                return true;
            }
            lock (_locker)
            {
                if (_rereadRequested)
                {
                    return true;
                }
            }
            return false;
        }
        private bool ShouldWeReadProcessFiles()
        {
            bool forceRead = false; // for debugging use
            if (forceRead || _lastReadProcesFilesUtc == DateTime.MinValue)
            {
                return true;
            }
            TimeSpan elapsed = DateTime.UtcNow - _lastReadProcesFilesUtc;
            if (elapsed > _rereadProcessFilesInterval)
            {
                return true;
            }
            lock (_locker)
            {
                if (_rereadRequested)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Check all process files mod dates, to see if recent
        /// Don't actually spend the time to read the files
        /// </summary>
        private void CheckLiveProcessFiles()
        {
            var deadGames = new List<GameSession>();
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                string heartbeatFile = gameSession.ProcessStatusFilepath;
                var status = GetStatusFromHeartbeatFileTime(gameSession);
                if (status == ServerAccountStatus.None)
                {
                    deadGames.Add(gameSession);
                }
                else
                {
                    if (gameSession.Status != status)
                    {
                        gameSession.Status = status;
                        NotifyGameChange(GameChangeType.ChangeStatus, gameSession);
                    }
                }
            }
            foreach (var deadGame in deadGames)
            {
                deadGame.Status = ServerAccountStatus.None;
                RemoveObsoleteHeartbeatFile(deadGame.ProcessId);
            }
        }
        /// <summary>
        /// Read all process files
        ///  Check if any characters have changed
        /// </summary>
        private void ReadProcessFiles()
        {
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                string heartbeatFile = gameSession.ProcessStatusFilepath;
                if (string.IsNullOrEmpty(heartbeatFile))
                {
                    // This occurs when launching game session
                    continue;
                }
                var response = MagFilter.LaunchControl.GetHeartbeatStatus(heartbeatFile);
                if (!response.IsValid)
                {
                    Log.WriteError(string.Format("Invalid contents in heartbeat file: {0}", heartbeatFile));
                    continue;
                }
                var status = GetStatusFromHeartbeatFileTime(gameSession);
                bool newGame = false;
                bool changedGame = false;
                bool changedStatus = false;
                if (gameSession.AccountName == null)
                {
                    newGame = true;
                }
                else if (gameSession.AccountName != response.Status.AccountName
                    || gameSession.ServerName != response.Status.ServerName)
                {
                    // This doesn't make sense and shouldn't happen
                    // Account & Server should be fixed for the life of a game session
                    Log.WriteError(string.Format("Account/Server change in heartbeat file!: {0}", heartbeatFile));
                    changedGame = true;
                }
                else if (gameSession.CharacterName != response.Status.CharacterName)
                {
                    changedGame = true;
                }
                else if (gameSession.Status != status)
                {
                    changedStatus = true;
                    gameSession.Status = status;
                }
                UpdateGameSessionFromHeartbeatStatus(gameSession, heartbeatFile, response);
                if (newGame)
                {
                    NotifyGameChange(GameChangeType.StartGame, gameSession);
                }
                else if (changedGame)
                {
                    NotifyGameChange(GameChangeType.ChangeGame, gameSession);
                }
                else if (changedStatus)
                {
                    NotifyGameChange(GameChangeType.ChangeStatus, gameSession);
                }
            }
        }
        private void UpdateGameSessionFromHeartbeatStatus(GameSession gameSession, 
            string filepath, MagFilter.LaunchControl.HeartbeatResponse response)
        {
            gameSession.ProcessStatusFilepath = filepath;
            if (!response.IsValid) { return; }
            gameSession.AccountName = response.Status.AccountName;
            gameSession.ServerName = response.Status.ServerName;
            gameSession.CharacterName = response.Status.CharacterName;
            if (gameSession.ProcessId != response.Status.ProcessId)
            {
                _map.SetGameSessionProcessId(gameSession, response.Status.ProcessId);
            }
            gameSession.UptimeSeconds = response.Status.UptimeSeconds;
        }
        private ServerAccountStatus GetStatusFromHeartbeatFileTime(GameSession gameSession)
        {
            if (gameSession.Status == ServerAccountStatus.Starting) { return ServerAccountStatus.Starting; }
            if (string.IsNullOrEmpty(gameSession.ProcessStatusFilepath))
            {
                if (gameSession.Status == ServerAccountStatus.Running)
                {
                    return ServerAccountStatus.Warning;
                }
                else
                {
                    return gameSession.Status;
                }
            }
            string heartbeatFile = gameSession.ProcessStatusFilepath;   
            DateTime writtenUtc = File.GetLastWriteTimeUtc(heartbeatFile);
            TimeSpan elapsed = (DateTime.UtcNow - writtenUtc);
            if (elapsed < _warningInterval)
            {
                return ServerAccountStatus.Running;
            }
            else
            {
                if (elapsed > _liveInterval)
                {
                    return ServerAccountStatus.None;
                }
                else
                {
                    return ServerAccountStatus.Warning;
                }
            }
        }
        private void CleanupOldProcessFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(MagFilter.FileLocations.GetRunningFolder());
            var filepathsToDelete = new List<string>();
            foreach (var fileInfo in dir.EnumerateFiles())
            {
                int processId = 0;
                if (fileInfo.Extension == ".txt")
                {
                    processId = MagFilter.FileLocations.GetProcessIdFromProcessDllToExeFilepath(fileInfo.Name);
                    TimeSpan elapsed = (DateTime.UtcNow - fileInfo.LastWriteTimeUtc);
                    if (elapsed < _liveInterval)
                    {
                        if (_map.HasGameSessionByProcessId(processId))
                        {
                            // This is a known game we are monitoring
                        }
                        else
                        {
                            // This is an unknown game
                            TryToAddGameFromHeartbeatFile(fileInfo.FullName, processId);
                        }
                    }
                    else
                    {
                        RemoveObsoleteHeartbeatFile(processId);
                        processId = 0;
                    }
                }
                if (processId == 0)
                {
                    // dead
                    filepathsToDelete.Add(fileInfo.FullName);
                }
                else
                {
                    // We found it & added it up above in TryToAddGameFromHeartbeatFile
                }
            }
            foreach (string filepath in filepathsToDelete)
            {
                File.Delete(filepath);
            }
            _lastCleanupUtc = DateTime.UtcNow;
        }
        private void TryToAddGameFromHeartbeatFile(string filepath, int processId)
        {
            var response = MagFilter.LaunchControl.GetHeartbeatStatus(filepath);
            if (response.IsValid)
            {
                var gameSession = _map.GetGameSessionByServerAccount(response.Status.ServerName, response.Status.AccountName);
                bool newGame = false;
                if (gameSession == null)
                {
                    gameSession = new GameSession();
                    newGame = true;
                }
                UpdateGameSessionFromHeartbeatStatus(gameSession, filepath, response);
                if (newGame)
                {
                    _map.AddGameSession(gameSession);
                }
                if (!_configurator.ContainsMagFilterPath(response.Status.MagFilterFilePath))
                {
                    _configurator.AddGameConfig(
                        new Configurator.GameConfig()
                        {
                            MagFilterPath = response.Status.MagFilterFilePath,
                            MagFilterVersion = response.Status.MagFilterVersion
                        }
                        );
                }
                NotifyGameChange(GameChangeType.StartGame, gameSession);
            }
        }
        private void RemoveObsoleteHeartbeatFile(int processId)
        {
            // obsolete heartbeat file
            var gameSession = _map.GetGameSessionByProcessId(processId);
            if (gameSession != null)
            {
                _map.RemoveGameSessionByProcessId(processId);
                gameSession.Status = ServerAccountStatus.None;
                NotifyGameChange(GameChangeType.EndGame, gameSession);
            }
        }
        private void NotifyGameChange(GameChangeType changeType, GameSession gameSession)
        {
            if (GameChangeEvent != null)
            {
                GameChangeEvent(changeType, gameSession);
            }
        }
    }
}
