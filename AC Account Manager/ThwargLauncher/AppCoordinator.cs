using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    /// <summary>
    /// This object launches the disparate systems and wires them up
    /// </summary>
    class AppCoordinator
    {
        private static AppCoordinator theAppCoordinator = null;
        private MainWindow _mainWindow;
        private MainWindowViewModel _mainViewModel;
        private WebService.WebServiceManager _webManager = new WebService.WebServiceManager();
        private GameSessionMap _gameSessionMap;
        private Configurator _configurator;
        private GameMonitor _gameMonitor;
        private UiGameMonitorBridge _uiGameMonitorBridge = null;
        private LogWriter _logWriter = null;
        private CommandManager _commandManager = null;
        private ServerMonitor _monitor = null;

        public AppCoordinator()
        {
            theAppCoordinator = this;

            BeginMonitoringGame();

            ShowMainWindow();

            BeginMonitoringServers();
        }
        public static void RemoveObsoleteProcess(int processId)
        {
            theAppCoordinator._gameMonitor.RemoveGameByPid(processId);
        }
        private void BeginMonitoringGame()
        {
            // Logger is a static object, so it already exists
            string logfilepath = GetLauncherLogPath();
            _logWriter = new LogWriter(logfilepath);
            _configurator = new Configurator();
            RecordGameDll();
            _gameSessionMap = new GameSessionMap();
            _mainViewModel = new MainWindowViewModel(_gameSessionMap, _configurator);
            _mainViewModel.RequestShowMainWindowEvent += () => _mainWindow.Show();
            _gameMonitor = new GameMonitor(_gameSessionMap, _configurator);
            _commandManager = new CommandManager(_gameMonitor, _gameSessionMap);
            bool testCommandTokenParser = true;
            if (testCommandTokenParser)
            {
                _commandManager.TestParse();
            }
            _uiGameMonitorBridge = new UiGameMonitorBridge(_gameMonitor, _mainViewModel);
            _uiGameMonitorBridge.Start();
            _gameMonitor.Start();
        }
        private void BeginMonitoringServers()
        {
            if (Properties.Settings.Default.ServerMonitorEnabled)
            {
                _monitor = new ServerMonitor();
                _monitor.StartMonitor(() => { return ServerManager.ServerList; });
            }
        }
        private void ShowMainWindow()
        {
            _mainWindow = new MainWindow(_mainViewModel, _gameSessionMap, _gameMonitor);
            _mainWindow.Closing += mainWindow_Closing;
            _mainWindow.Show();
            if (Properties.Settings.Default.LastUsedSimpleLaunch)
            {
                _mainViewModel.DisplaySimpleLauncher();
            }
        }
        internal string GetLauncherLogPath()
        {
            string filepath = System.IO.Path.Combine(MagFilter.FileLocations.AppLogsFolder,  "ThwargLauncher-%PID%_log.txt");
            filepath = MagFilter.FileLocations.ExpandFilepath(filepath);
            MagFilter.FileLocations.CreateAnyNeededFoldersOfFile(filepath);
            return filepath;
        }
        private void RecordGameDll()
        {
            var info = MagFilter.LaunchControl.GetMagFilterInfo();
            _configurator.AddGameConfig(
                new Configurator.GameConfig()
                {
                    MagFilterPath = info.MagFilterPath,
                    MagFilterVersion = info.MagFilterVersion
                }
                );
        }
        void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mainViewModel.ShutSubsidiaryWindows();
            EndMonitoringGame();
            if (_monitor != null)
            {
                _monitor.StopMonitor();
            }
        }
        private void EndMonitoringGame()
        {
            _uiGameMonitorBridge.Stop();
            _gameMonitor.RemoveAllSessions();
            _gameMonitor.Stop();
        }
        public static GameSession GetTheGameSessionByServerAccount(string serverName, string accountName)
        {
            // architectural problem getting to the game session map
            return theAppCoordinator._gameSessionMap.GetGameSessionByServerAccount(serverName: serverName, accountName: accountName);
        }
    }
}
