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
        private readonly GameStatusMap _map;
        private Configurator _configurator;
        private TimeSpan _liveInterval = new TimeSpan(0, 1, 0); // must be written this recently to be alive
        private DateTime _lastCleanupUtc = DateTime.MinValue;
        private TimeSpan _cleanupInterval = new TimeSpan(0, 5, 0); // 5 minutes
        private DateTime _lastReadProcesFilesUtc = DateTime.MinValue;
        private TimeSpan _rereadProcessFilesInterval = new TimeSpan(0, 1, 0); // 5 minutes
        private bool _rereadRequested = false; // cross-thread access
        private bool _isWorking = false; // reentrancy guard

        public enum GameChangeType { StartGame, EndGame, ChangeGame };
        public delegate void GameChangeHandler(GameChangeType changeType, GameStatus gameStatus);
        public event GameChangeHandler GameChangeEvent;

        public GameMonitor(GameStatusMap map, Configurator configurator)
        {
            _map = map;
            _configurator = configurator;
        }
        public void Start() // main thread
        {
            int seconds = ConfigSettings.GetConfigInt("HeartbeatFailedTimeoutSeconds", 60);
            _liveInterval = TimeSpan.FromSeconds(seconds);

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
            var deadGames = new List<GameStatus>();
            foreach (var gameStatus in _map.GetAllGameStatuses())
            {
                string heartbeatFile = gameStatus.ProcessStatusFilepath;
                DateTime writtenUtc = File.GetLastWriteTimeUtc(heartbeatFile);
                TimeSpan elapsed = (DateTime.UtcNow - writtenUtc);
                if (elapsed > _liveInterval)
                {
                    deadGames.Add(gameStatus);
                }
            }
            foreach (var deadGame in deadGames)
            {
                RemoveObsoleteHeartbeatFile(deadGame.ProcessId);
            }
        }
        /// <summary>
        /// Read all process files
        ///  Check if any characters have changed
        /// </summary>
        private void ReadProcessFiles()
        {
            foreach (var gameStatus in _map.GetAllGameStatuses())
            {
                string heartbeatFile = gameStatus.ProcessStatusFilepath;
                var response = MagFilter.LaunchControl.GetHeartbeatStatus(heartbeatFile);
                if (response.IsValid)
                {
                    if (gameStatus.AccountName == null)
                    {
                        // newly found
                        gameStatus.AccountName = response.Status.AccountName;
                        gameStatus.ServerName = response.Status.ServerName;
                        gameStatus.CharacterName = response.Status.CharacterName;
                        gameStatus.UptimeSeconds = response.Status.UptimeSeconds;
                        NotifyGameChange(GameChangeType.StartGame, gameStatus);
                    }
                    if (gameStatus.AccountName != response.Status.AccountName
                        || gameStatus.ServerName != response.Status.ServerName)
                    {
                        Log.WriteError(string.Format("Account/Server change in heartbeat file!: {0}", heartbeatFile));
                        gameStatus.AccountName = response.Status.AccountName;
                        gameStatus.ServerName = response.Status.ServerName;
                        gameStatus.UptimeSeconds = response.Status.UptimeSeconds;
                        NotifyGameChange(GameChangeType.ChangeGame, gameStatus);
                    }
                    if (gameStatus.CharacterName != response.Status.CharacterName)
                    {
                        gameStatus.CharacterName = response.Status.CharacterName;
                        gameStatus.UptimeSeconds = response.Status.UptimeSeconds;
                        NotifyGameChange(GameChangeType.ChangeGame, gameStatus);
                    }
                }
                else
                {
                    Log.WriteError(string.Format("Invalid contents in heartbeat file: {0}", heartbeatFile));
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
                    if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < _liveInterval)
                    {
                        if (_map.HasGameStatusByProcessId(processId))
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
                var gameStatus = new GameStatus();
                gameStatus.AccountName = response.Status.AccountName;
                gameStatus.CharacterName = response.Status.CharacterName;
                gameStatus.ServerName = response.Status.ServerName;
                gameStatus.ProcessId = processId;
                gameStatus.UptimeSeconds = response.Status.UptimeSeconds;
                gameStatus.ProcessStatusFilepath = filepath;
                _map.AddGameStatus(gameStatus);
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
                NotifyGameChange(GameChangeType.StartGame, gameStatus);
            }
        }
        private void RemoveObsoleteHeartbeatFile(int processId)
        {
            // obsolete heartbeat file
            var gameStatus = _map.GetGameStatusByProcessId(processId);
            if (gameStatus != null)
            {
                _map.RemoveGameStatusByProcessId(processId);
                NotifyGameChange(GameChangeType.EndGame, gameStatus);
            }
        }
        private void NotifyGameChange(GameChangeType changeType, GameStatus gameStatus)
        {
            if (GameChangeEvent != null)
            {
                GameChangeEvent(changeType, gameStatus);
            }
        }
    }
}
