using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        private const int GAMECLIENTLOCATIONS_CHECK_SECONDS = 10;
        private DateTime _lastReadProcesFilesUtc = DateTime.MinValue;
        private TimeSpan _rereadProcessFilesInterval = new TimeSpan(0, 1, 0); // 1 minute
        private DateTime _lastReadServerStatsUtc = DateTime.MinValue;
        private TimeSpan _rereadServerStatsInterval = new TimeSpan(0, 10, 0); // 10 minutes
        private DateTime _lastUpdateUi = DateTime.MinValue;
        private bool _rereadRequested = false; // cross-thread access
        private bool _isWorking = false; // reentrancy guard
        private DateTime _lastWork;
        public event Action CharacterFileChanged;
        private DateTime _lastCheckedCharacterFileUtc = DateTime.MinValue;
        private DateTime _characterFileTimeUtc = DateTime.MinValue;
        private DateTime _lastOnlineTimeUtc = DateTime.MinValue;
        private DateTime _lastCheckedGameClientLocationsUtc = DateTime.MinValue;

        public enum GameChangeType { StartGame, EndGame, ChangeGame, ChangeStatus, ChangeTeam, ChangeNone };
        public delegate void GameChangeHandler(GameSession gameSession, GameChangeType changeType);
        public event GameChangeHandler GameChangeEvent;
        public delegate void GameCommandHandler(GameSession gameSession, string command);
        public event GameCommandHandler GameCommandEvent;

        public event EventHandler GameDiedEvent;
        protected void OnGameDied(EventArgs e)
        {
            if (GameDiedEvent != null)
            {
                GameDiedEvent(this, e);
            }
        }

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
                    PerformCleanupOldProcessFiles();
                }
                if (ShouldWeReadProcessFiles())
                {
                    PerformReadProcessFiles();
                }
                if (ShouldWeReadServerStats())
                {
                    PerformReadServerStats();
                }
                if (ShouldWeCheckCharacterFile())
                {
                    PerformCheckCharacterFile();
                }
                if (ShouldWeCheckGameClientLocations())
                {
                    PerformCheckGameClientLocations();
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
        private bool ShouldWeReadServerStats()
        {
            bool forceRead = false; // for debugging
            if (forceRead || _lastReadServerStatsUtc == DateTime.MinValue)
            {
                _lastReadServerStatsUtc = DateTime.UtcNow;
                return true;
            }
            TimeSpan elapsed = DateTime.UtcNow - _lastReadServerStatsUtc;
            if (elapsed > _rereadServerStatsInterval)
            {
                _lastReadServerStatsUtc = DateTime.UtcNow;
                return true;
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
        private void PerformCheckCharacterFile()
        {
            foreach (string filepath in ThwargFilter.FileLocations.GetAllCharacterFilePaths())
            {
                DateTime fileTimeUtc = File.GetLastWriteTimeUtc(filepath);
                if (fileTimeUtc > _characterFileTimeUtc)
                {
                    _characterFileTimeUtc = fileTimeUtc;
                    // character file changed
                    if (CharacterFileChanged != null)
                    {
                        CharacterFileChanged();
                        break; // event handler will reread them all so no need to check the rest
                    }
                }
            }
            _lastCheckedCharacterFileUtc = DateTime.UtcNow;
        }
        private bool ShouldWeCheckGameClientLocations()
        {
            var elapsed = (DateTime.UtcNow - _lastCheckedGameClientLocationsUtc);
            return (elapsed.TotalSeconds > GAMECLIENTLOCATIONS_CHECK_SECONDS);
        }
        private void PerformCheckGameClientLocations()
        {
            foreach (var session in _map.GetAllGameSessions())
            {
                if (session.Status != ServerAccountStatusEnum.Running)
                    continue;
                
                if (session.WindowHwnd == null || session.WindowHwnd == (IntPtr)0)
                {
                    if (session.ProcessId != 0)
                    {
                        LookForGameWindow(session);
                    }
                    continue;
                }
                if (!session.hasRestoredWindowLocation)
                {
                    TryToRestoreSessionPlacementInfo(session);
                    session.hasRestoredWindowLocation = true;
                }
                else
                {
                    TryToSaveSessionPlacementInfo(session);
                }
            }
            _lastCheckedGameClientLocationsUtc = DateTime.UtcNow;
        }
        private void TryToRestoreSessionPlacementInfo(GameSession session)
        {
            bool restoreWindows = Properties.Settings.Default.RestoreGameWindows;
            if (!restoreWindows) { return; }
            string key = GameMonitor.GetSessionSettingsKey(Server: session.ServerName, Account: session.AccountName);
            var settings = PersistenceHelper.SettingsFactory.Get();
            string placementString = settings.GetString(key);
            IntPtr hwnd = session.WindowHwnd;
            var prevPlacement = WindowPlacementUtil.WindowPlacement.GetPlacementFromString(placementString);
            if (prevPlacement.length > 0)
            {
                var placementInfo = WindowPlacementUtil.WindowPlacement.GetPlacementInfo(hwnd);
                if (AreSameNormalSize(prevPlacement, placementInfo.Placement))
                {
                    Logger.WriteDebug("Windows are same normal size.");
                    WindowPlacementUtil.WindowPlacement.SetPlacement(hwnd, prevPlacement);
                }
                else
                {
                    Logger.WriteDebug("Windows are not the same normal size.");
                    Logger.WriteDebug("PREVPLACEMENT - Height:" + GetNormalHeight(prevPlacement) + " width:" + GetNormalWidth(prevPlacement));
                    Logger.WriteDebug("PLACEMENT - Height:" + GetNormalHeight(placementInfo.Placement) + " width:" + GetNormalWidth(placementInfo.Placement));
                }
            }
            Logger.WriteDebug("Restored game position server: {0}, account: {1}", session.ServerName, session.AccountName);

        }
        private static bool AreSameNormalSize(WindowPlacementUtil.WINDOWPLACEMENT placement1, WindowPlacementUtil.WINDOWPLACEMENT placement2)
        {
            return GetNormalHeight(placement1) == GetNormalHeight(placement2) && GetNormalWidth(placement1) == GetNormalWidth(placement2);
        }
        private static int GetNormalHeight(WindowPlacementUtil.WINDOWPLACEMENT placement) { return placement.normalPosition.Bottom - placement.normalPosition.Top; }
        private static int GetNormalWidth(WindowPlacementUtil.WINDOWPLACEMENT placement) { return placement.normalPosition.Right - placement.normalPosition.Left; }
        private void TryToSaveSessionPlacementInfo(GameSession session)
        {
            if (!Properties.Settings.Default.SaveGameWindows) { return; }
            var placementInfo = WindowPlacementUtil.WindowPlacement.GetPlacementInfo(session.WindowHwnd);
            if (placementInfo.IsEmpty())
            {
                return;
            }
            string placementString = placementInfo.PlacementString;
            if (placementString == session.WindowPlacementString)
            {
                return;
            }
            string key = GetSessionSettingsKey(session);
            var settings = PersistenceHelper.SettingsFactory.Get();
            settings.SetString(key, placementString);
            settings.Save();
            session.WindowPlacementString = placementString;
        }
        private void LookForGameWindow(GameSession session)
        {
            var finder = new ThwargUtils.WindowFinder();
            IntPtr hwnd = finder.FindWindowByCaptionAndProcessId(regex: null, newWindow: false, processId: session.ProcessId);
            if (hwnd != (IntPtr)0)
            {
                // Only save hwnd if window has been renamed, meaning launch completed
                string gameCaptionPattern = ConfigSettings.GetConfigString("GameCaptionPattern", null);
                string caption = ThwargUtils.WindowFinder.GetWindowTextString(hwnd);
                if (caption != gameCaptionPattern)
                {
                    session.WindowHwnd = hwnd;
                }
            }

        }
        private string GetSessionSettingsKey(GameSession session)
        {
            return GetSessionSettingsKey(session.ServerName, session.AccountName);
        }
        public static string GetSessionSettingsKey(string Server, string Account)
        {
            return string.Format("s:{0}-a:{1}", Server, Account);
        }
        private void SendAndReceiveCommands()
        {
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                if (gameSession.GameChannel != null)
                {
                    if (gameSession.GameChannel.NeedsToWrite)
                    {
                        var writer = new ThwargFilter.Channels.ChannelWriter();
                        writer.WriteCommandsToFile(gameSession.GameChannel);
                    }
                    if (true)
                    {
                        var writer = new ThwargFilter.Channels.ChannelWriter();
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
        private void PerformReadProcessFiles()
        {
            foreach (var gameSession in _map.GetAllGameSessions())
            {
                string heartbeatFile = gameSession.ProcessStatusFilepath;
                if (string.IsNullOrEmpty(heartbeatFile))
                {
                    // This occurs when launching game session
                    continue;
                }
                var response = ThwargFilter.LaunchControl.GetHeartbeatStatus(heartbeatFile);
                if (!response.IsValid)
                {
                    Logger.WriteError(string.Format("Invalid contents in heartbeat file: {0}", heartbeatFile));
                    continue;
                }
                var status = GetStatusFromHeartbeatFileTime(gameSession);

                if (!response.Status.IsOnline)
                {
                    int gameInteractionTimeoutSeconds = ConfigSettings.GetConfigInt("GameInteractionTimeoutSeconds", 120);
                    // ThwargFilter reports !IsOnline if server dispatch quits firing
                    // but that isn't reliable, as it doesn't fire when not logged in to a character
                    if (response.Status.LastServerDispatchSecondsAgo > gameInteractionTimeoutSeconds)
                    {
                        status = ServerAccountStatusEnum.None;
                        Logger.WriteInfo("Killing offline/character screen game");
                    }
                }
                else
                {
                    _lastOnlineTimeUtc = DateTime.UtcNow;
                }

                if (status == ServerAccountStatusEnum.None)
                {
                    Process p = TryGetProcessFromId(response.Status.ProcessId);
                    if (p != null)
                    {
                        p.Kill();
                        File.Delete(heartbeatFile);
                        _map.RemoveGameSessionByProcessId(p.Id);
                        Logger.WriteDebug("Killing process: " + p.Id.ToString());
                        OnGameDied(new EventArgs());
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
        class ServerPlayerCount { public string server; public string date; public int count; public string age; }
        private void PerformReadServerStats()
        {
            try
            {
                string res;
                string url = Properties.Settings.Default.ServerCountUrl;
                using (var wc = new WebClient())
                {
                    string datastr = wc.DownloadString(url);
                    dynamic dyn = JsonConvert.DeserializeObject(datastr);
                    foreach (var obj in dyn)
                    {
                        var data = new ServerPlayerCount();
                        data.count = int.Parse(obj["count"].ToString());
                        data.age = obj["age"].ToString();
                        data.server = obj["server"].ToString();
                        data.date = obj["date"].ToString();

                        // TODO - think about this cross-thread call
                        ServerManager.UpdatePlayerCount(data.server, data.count, data.age);
                    }
                }
            }
            catch (Exception exc)
            {
                // hmm
            }
        }
        private static Process TryGetProcessFromId(int pid)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                return p;
            }
            catch
            {
                return null;
            }
        }
        private void UpdateGameSessionFromHeartbeatStatus(GameSession gameSession,
        string filepath, ThwargFilter.LaunchControl.HeartbeatResponse response)
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
            gameSession.GameChannel = ThwargFilter.Channels.Channel.MakeLauncherChannel(processId);
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
        private void PerformCleanupOldProcessFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(ThwargFilter.FileLocations.GetRunningFolder());
            var filepathsToDelete = new List<string>();
            foreach (var fileInfo in dir.EnumerateFiles("game_*.txt"))
            {
                int processId = 0;
                if (fileInfo.Extension == ".txt")
                {
                    processId = ThwargFilter.FileLocations.GetProcessIdFromGameHeartbeatFilepath(fileInfo.Name);
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
            var response = ThwargFilter.LaunchControl.GetHeartbeatStatus(filepath);
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
                if (!_configurator.ContainsThwargFilterPath(response.Status.ThwargFilterFilePath))
                {
                    var gameConfig = new Configurator.GameConfig()
                    {
                        ThwargFilterPath = response.Status.ThwargFilterFilePath,
                        ThwargFilterVersion = response.Status.ThwargFilterVersion
                    };
                    _configurator.AddGameConfig(gameConfig);
                    Logger.WriteInfo(string.Format(
                        "ThwargFilter#{0} found: {1}",
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
                if (File.Exists(gamePath))
                {
                    TryToDeleteFile(gamePath);
                }
            }
        }
        private void TryToDeleteFile(string filepath)
        {
            for (int i = 0; i < 5; ++i)
            {
                try
                {
                    File.Delete(filepath);
                    return;
                }
                catch
                {
                }
                System.Threading.Thread.Sleep(100);
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
        private void NotifyGameCommand(GameSession gameSession, string command)
        {
            if (GameCommandEvent != null)
            {
                GameCommandEvent(gameSession, command);
            }
        }
        public void KillSessionAndNotify(GameSession inboundGameSession)
        {
            var pid = inboundGameSession.ProcessId;
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(pid);
                process.Kill();
                this.RemoveGameByPidAndNotifyLauncher(pid);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Exception killing session pid {0}: {1}", pid, exc);
            }

        }
        public void KillAllSessionsAndNotify()
        {
            foreach(var session in _map.GetAllGameSessions())
            {
                KillSessionAndNotify(session);
            }
        }
        public void DisableWindowPosition(GameSession inboundGameSession)
        {
            Properties.Settings.Default.SaveGameWindows = false;
            Properties.Settings.Default.RestoreGameWindows = false;
            Properties.Settings.Default.Save();
        }
        public void LockWindowPosition(GameSession inboundGameSession)
        {
            TryToSaveSessionPlacementInfo(inboundGameSession);
            Properties.Settings.Default.SaveGameWindows = false;
            Properties.Settings.Default.RestoreGameWindows = true;
            Properties.Settings.Default.Save();
        }
        public void UnlockWindowPosition(GameSession inboundGameSession)
        {
            Properties.Settings.Default.SaveGameWindows = true;
            Properties.Settings.Default.RestoreGameWindows = true;
            Properties.Settings.Default.Save();
        }
        public void RemoveGameByPid(int processId)
        {
            RemoveDeadSessionByPid(processId);
        }
        public void RemoveGameByPidAndNotifyLauncher(int processId)
        {
            RemoveDeadSessionByPid(processId);
            OnGameDied(new EventArgs());
        }
    }
}
