using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    /// <summary>
    /// The GameMonitor runs its own thread to read & write data to the running games
    /// It fires events to report commands & game status changes
    /// </summary>
    class GameMonitor
    {
        private static object _locker = new object();

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly GameSessionMap _map;
        private readonly Configurator _configurator;
        private TimeSpan _liveInterval; // must be written this recently to be alive
        private TimeSpan _warningInterval; // must be written this recently to be alive
        private DateTime _lastCleanupUtc = DateTime.MinValue;
        private TimeSpan _cleanupInterval = new TimeSpan(0, 5, 0); // 5 minutes
        private const int TIMER_SECONDS = 3;
        private const int CHARACTERFILE_CHECK_SECONDS = 30;
        private DateTime _lastReadProcesFilesUtc = DateTime.MinValue;
        private TimeSpan _rereadProcessFilesInterval = new TimeSpan(0, 1, 0); // 1 minute
        private DateTime _lastUpdateUi = DateTime.MinValue;
        private bool _rereadRequested = false; // cross-thread access
        private bool _isWorking = false; // reentrancy guard
        private DateTime _lastWork;
        public event Action CharacterFileChanged;
        private DateTime _lastCheckedCharacterFileUtc = DateTime.MinValue;
        private DateTime _characterFileTimeUtc = DateTime.MinValue;

        public enum GameChangeType { StartGame, EndGame, ChangeGame, ChangeStatus, ChangeTeam, ChangeNone };
        public delegate void GameChangeHandler(GameSession gameSession, GameChangeType changeType);
        public event GameChangeHandler GameChangeEvent;
        public delegate void GameCommandHandler(GameSession gameSession, string command);
        public event GameCommandHandler GameCommandEvent;

        public GameMonitor(GameSessionMap map, Configurator configurator)
        {
            if (map == null) { throw new Exception("Null GameSessionMap in GameMonitor()"); }
            if (configurator == null) { throw new Exception("Null Configurator in GameMonitor()"); }

            _map = map;
            _configurator = configurator;
        }
        public void Start() // main thread
        {
            _liveInterval = TimeSpan.FromSeconds(ConfigSettings.GetConfigInt("HeartbeatFailedTimeoutSeconds", 60));
            _warningInterval = TimeSpan.FromSeconds(ConfigSettings.GetConfigInt("HeartbeatWarningTimeoutSeconds", 15));

            int intervalMilliseconds = 1000 * TIMER_SECONDS;
            _timer.Interval = intervalMilliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            _map.CommandsReceivedEvent += MapCommandsReceivedEvent;
        }

        void MapCommandsReceivedEvent(GameSession session)
        {
            Logger.WriteError("QWQ Thwarg Received channel Watch Event");
            _rereadRequested = true;
            PerformWork();
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
            // Avoid doing work again if we did it recently (presumably from map event)
            if ((DateTime.UtcNow - _lastWork).TotalMilliseconds < this._timer.Interval / 2) { return; }
            PerformWork();
        }
        private void PerformWork()
        {
            // Avoid reentrancy
            if (_isWorking) { return; }
            try
            {
                _isWorking = true;
                if (ShouldWeCleanup())
                {
                    CleanupOldProcessFiles();
                }
                if (ShouldWeReadProcessFiles())
                {
                    ReadProcessFiles();
                }
                if (ShouldWeCheckCharacterFile())
                {
                    CheckCharacterFile();
                }
                CheckLiveProcessFiles();
                SendAndReceiveCommands();
                ProcessAnyPendingCommnds();
                UpdateUiIfNeeded();
                _lastWork = DateTime.UtcNow;
            }
            finally
            {
                _isWorking = false;
            }
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
        /// </summary>
        private void CheckLiveProcessFiles()
        {
            var deadGames = new List<GameSession>();
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                string heartbeatFile = gameSession.ProcessStatusFilepath;
                var status = GetStatusFromHeartbeatFileTime(gameSession);
                if (status == ServerAccountStatusEnum.None)
                {
                    Logger.WriteDebug("Found dead game: {0}", gameSession.Description);
                    deadGames.Add(gameSession);
                }
                else
                {
                    // Handle orphan games that never got a heartbeat
                    if (status == ServerAccountStatusEnum.Warning && gameSession.LastGoodStatusUtc == DateTime.MinValue)
                    {
                        status = ServerAccountStatusEnum.None;
                    }
                    if (gameSession.Status != status)
                    {
                        Logger.WriteDebug("Found orphan game {0}, changing status from {1} to {2}", gameSession.Description, gameSession.Status, status);
                        gameSession.Status = status;
                        NotifyGameChange(gameSession, GameChangeType.ChangeStatus);
                    }
                }
            }
            foreach (var deadGame in deadGames)
            {
                deadGame.Status = ServerAccountStatusEnum.None;
                Logger.WriteDebug("Removing dead game: {0}", deadGame.Description);
                RemoveSessionByPidKey(deadGame.ProcessIdKey);
            }
        }
        private bool ShouldWeCheckCharacterFile()
        {
            var elapsed = (DateTime.UtcNow - _lastCheckedCharacterFileUtc);
            return (elapsed.TotalSeconds > CHARACTERFILE_CHECK_SECONDS);
        }
        private void CheckCharacterFile()
        {
            string filepath = MagFilter.FileLocations.GetCharacterFilePath();
            if (File.Exists(filepath))
            {
                DateTime fileTimeUtc = File.GetLastWriteTimeUtc(filepath);
                if (fileTimeUtc > _characterFileTimeUtc)
                {
                    _characterFileTimeUtc = fileTimeUtc;
                    // character file changed
                    if (CharacterFileChanged != null)
                    {
                        CharacterFileChanged();
                    }
                }
            }
            _lastCheckedCharacterFileUtc = DateTime.UtcNow;
        }
        private void SendAndReceiveCommands()
        {
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                if (gameSession.GameChannel != null)
                {
                    if (gameSession.GameChannel.NeedsToWrite)
                    {
                        var writer = new MagFilter.Channels.ChannelWriter();
                        writer.WriteCommandsToFile(gameSession.GameChannel);
                    }
                    if (true)
                    {
                        var writer = new MagFilter.Channels.ChannelWriter();
                        writer.ReadCommandsFromFile(gameSession.GameChannel);
                    }
                }
            }
        }
        private void ProcessAnyPendingCommnds()
        {
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                if (gameSession.GameChannel != null)
                {
                    var cmd = gameSession.GameChannel.DequeueInbound();
                    if (cmd != null && cmd.CommandString != null)
                    {
                        NotifyGameCommand(gameSession, cmd.CommandString);
                    }
                }
            }
        }
        private void UpdateUiIfNeeded()
        {
            if (_lastUpdateUi != DateTime.MinValue && DateTime.UtcNow - _lastUpdateUi < TimeSpan.FromSeconds(20))
            {
                return;
            }
            // periodic update to UI to get account summaries changing
            foreach (var session in _map.GetAllGameSessions())
            {
                if (session.GameChannel != null)
                {
                    NotifyGameChange(session, GameChangeType.ChangeNone);
                }
            }
            _lastUpdateUi = DateTime.UtcNow;
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
                    Logger.WriteError(string.Format("Invalid contents in heartbeat file: {0}", heartbeatFile));
                    continue;
                }
                var status = GetStatusFromHeartbeatFileTime(gameSession);
                
                if(!response.Status.IsOnline)
                {
                    status = ServerAccountStatusEnum.None;
                }

                if(status == ServerAccountStatusEnum.None)
                {
                    Process p = Process.GetProcessById(response.Status.ProcessId);
                    if (p != null)
                    {
                        p.Kill();
                        Logger.WriteDebug("Killing process: " + p.Id.ToString());
                    }
                }

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
                    Logger.WriteError(string.Format("Account/Server change in heartbeat file!: {0}", heartbeatFile));
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
                    NotifyGameChange(gameSession, GameChangeType.StartGame);
                }
                else if (changedGame)
                {
                    NotifyGameChange(gameSession, GameChangeType.ChangeGame);
                }
                else if (changedStatus)
                {
                    NotifyGameChange(gameSession, GameChangeType.ChangeStatus);
                }
            }
            _lastReadProcesFilesUtc = DateTime.UtcNow;
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
                int oldpid = gameSession.ProcessId;
                _map.SetGameSessionProcessId(gameSession, response.Status.ProcessId);
            }
            if (gameSession.GameChannel == null)
            {
                CreateGameChannel(response.Status.ProcessId, gameSession);
            }
            gameSession.UptimeSeconds = response.Status.UptimeSeconds;
            if (gameSession.TeamList != response.Status.TeamList)
            {
                gameSession.AssignTeamSetFromString(response.Status.TeamList);
                NotifyGameChange(gameSession, GameChangeType.ChangeTeam);
            }
        }
        private void CreateGameChannel(int processId, GameSession gameSession)
        {
            gameSession.GameChannel = MagFilter.Channels.Channel.MakeLauncherChannel(processId);
            _map.StartSessionWatcher(gameSession);
        }
        private ServerAccountStatusEnum GetStatusFromHeartbeatFileTime(GameSession gameSession)
        {
            if (gameSession.Status == ServerAccountStatusEnum.Starting) { return ServerAccountStatusEnum.Starting; }
            if (string.IsNullOrEmpty(gameSession.ProcessStatusFilepath))
            {
                if (gameSession.Status == ServerAccountStatusEnum.Running)
                {
                    return ServerAccountStatusEnum.Warning;
                }
                else
                {
                    return gameSession.Status;
                }
            }
            string heartbeatFile = gameSession.ProcessStatusFilepath;   
            DateTime writtenUtc = File.GetLastWriteTimeUtc(heartbeatFile);
            if (writtenUtc > gameSession.LastGoodStatusUtc)
            {
                gameSession.LastGoodStatusUtc = writtenUtc;
            }
            TimeSpan elapsed = (DateTime.UtcNow - writtenUtc);
            if (elapsed < _warningInterval)
            {
                return ServerAccountStatusEnum.Running;
            }
            else
            {
                if (elapsed > _liveInterval)
                {
                    return ServerAccountStatusEnum.None;
                }
                else
                {
                    return ServerAccountStatusEnum.Warning;
                }
            }
        }
        private void CleanupOldProcessFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(MagFilter.FileLocations.GetRunningFolder());
            var filepathsToDelete = new List<string>();
            foreach (var fileInfo in dir.EnumerateFiles("game_*.txt"))
            {
                int processId = 0;
                if (fileInfo.Extension == ".txt")
                {
                    processId = MagFilter.FileLocations.GetProcessIdFromGameHeartbeatFilepath(fileInfo.Name);
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
                        Logger.WriteDebug("Killing game because elapsed {0:0} not less than liveInterval {1:0} - file {2}",
                            elapsed.TotalSeconds, _liveInterval.TotalSeconds, fileInfo.Name);
                        RemoveDeadSessionByPid(processId);
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
            foreach (var fileInfo in dir.EnumerateFiles("incmds_*.txt"))
            {
                TimeSpan elapsed = (DateTime.UtcNow - fileInfo.LastWriteTimeUtc);
                if (elapsed > _liveInterval)
                {
                    filepathsToDelete.Add(fileInfo.FullName);
                }
            }
            foreach (var fileInfo in dir.EnumerateFiles("outcmds_*.txt"))
            {
                TimeSpan elapsed = (DateTime.UtcNow - fileInfo.LastWriteTimeUtc);
                if (elapsed > _liveInterval)
                {
                    filepathsToDelete.Add(fileInfo.FullName);
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
                    var gameConfig = new Configurator.GameConfig()
                        {
                            MagFilterPath = response.Status.MagFilterFilePath,
                            MagFilterVersion = response.Status.MagFilterVersion
                        };
                    _configurator.AddGameConfig(gameConfig);
                    Logger.WriteInfo(string.Format(
                        "MagFilter#{0} found: {1}",
                        _configurator.GetNumberGameConfigs(),
                        gameConfig));
                }
                NotifyGameChange(gameSession, GameChangeType.StartGame);
            }
        }
        private void RemoveDeadSessionByPid(int processId)
        {
            string pidkey = GameSessionMap.GetProcessIdKey(processId);
            RemoveSessionByPidKey(pidkey);
        }
        private void RemoveSessionByPidKey(string pidkey)
        {
            var gameSession = _map.RemoveGameSessionByProcessIdKey(pidkey);
            if (gameSession != null)
            {
                gameSession.StopSessionWatcher();
                gameSession.Status = ServerAccountStatusEnum.None;
                NotifyGameChange(gameSession, GameChangeType.EndGame);
                string gamePath = gameSession.ProcessStatusFilepath;
                if(File.Exists(gamePath))
                {
                    Logger.WriteDebug("Deleting game {0}", gameSession.Description);
                    File.Delete(gamePath);
                }
            }
        }
        /// <summary>
        /// Remove all game sessions
        /// This is used to stop all the file watches
        /// </summary>
        public void RemoveAllSessions()
        {
            var keys = _map.GetAllProcessIdKeys().ToList();
            foreach (var key in keys)
            {
                RemoveSessionByPidKey(key);
            }
        }
        private void NotifyGameChange(GameSession gameSession, GameChangeType changeType)
        {
            if (GameChangeEvent != null)
            {
                GameChangeEvent(gameSession, changeType);
            }
        }
        private void NotifyGameCommand( GameSession gameSession, string command)
        {
            if (GameCommandEvent != null)
            {
                GameCommandEvent(gameSession, command);
            }
        }

        public void RemoveGameByPid(int processId)
        {
            RemoveDeadSessionByPid(processId);
        }
    }
}
