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
        private AccountManager _accountManager = null;
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

            ParseCommandLine();

            BeginMonitoringGame();

            ShowMainWindow();

            BeginMonitoringServers();
        }
        private void ParseCommandLine()
        {
            try
            {
                var switches = new CSharpCLI.Argument.SwitchCollection();
                switches.Add(new CSharpCLI.Argument.Switch("Profile", numberArguments: 1, isRequired: false));
                switches.Add(new CSharpCLI.Argument.Switch("AutoRelaunch", hasArguments: true, isRequired: false));
                var args = System.Environment.GetCommandLineArgs();
                var parser = new CSharpCLI.Parse.ArgumentParser(args, switches);
                parser.Parse();
                if (parser.IsParsed("Profile"))
                {
                    string profileName = parser.GetValue("Profile");
                    Properties.Settings.Default.LastProfileName = profileName;
                    Properties.Settings.Default.Save();
                }
                if (parser.IsParsed("AutoRelaunch"))
                {
                    bool relaunchChoice = true;
                    var setting = parser.GetValues("AutoRelaunch");
                    if (setting.Length > 0)
                    {
                        string value = parser.GetValue("AutoRelaunch", 1);
                        relaunchChoice = PersistenceHelper.AppSettings.ObjToBool(value, true);
                    }
                    Properties.Settings.Default.AutoRelaunch = relaunchChoice;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Error reading command line arguments", exc);
            }
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
            _gameMonitor = new GameMonitor(_gameSessionMap, _configurator);
            _accountManager = new AccountManager(_gameMonitor);
            _mainViewModel = new MainWindowViewModel(_accountManager, _gameSessionMap, _configurator);
            _mainViewModel.RequestShowMainWindowEvent += () => _mainWindow.Show();
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
