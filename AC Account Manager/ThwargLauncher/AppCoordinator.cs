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
        private MainWindowViewModel _viewModel;
        private WebService.WebServiceManager _webManager = new WebService.WebServiceManager();
        private GameSessionMap _gameSessionMap;
        private Configurator _configurator;
        private GameMonitor _gameMonitor;
        private UiGameMonitorBridge _uiGameMonitorBridge = null;
        private LogWriter _logWriter = null;
        private CommandManager _commandManager = null;

        public AppCoordinator()
        {
            theAppCoordinator = this;

            BeginMonitoringGame();

            ShowMainWindow();
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
            _viewModel = new MainWindowViewModel(_gameSessionMap, _configurator);
            _gameMonitor = new GameMonitor(_gameSessionMap, _configurator);
            _commandManager = new CommandManager(_gameMonitor, _gameSessionMap);
            TestParse();
            _uiGameMonitorBridge = new UiGameMonitorBridge(_gameMonitor, _viewModel);
            _uiGameMonitorBridge.Start();
            _gameMonitor.Start();
        }
        private void TestParse()
        {
            IList<string> result = null;
            result = TestParseLine("a b c");
            result = TestParseLine("a bde c");
            result = TestParseLine("a 'b c'");
            result = TestParseLine("a b 'c' \"alpha\"");
            
        }
        private IList<string> TestParseLine(string text)
        {
            IList<string> list = _commandManager.ParseTokens(text);
            string result = string.Join("], [", list.ToArray());
            return list;

        }
        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow(_viewModel, _gameSessionMap, _gameMonitor);
            mainWindow.Closing += mainWindow_Closing;
            mainWindow.Show();
        }
        internal string GetLauncherLogPath()
        {
            string filepath = MagFilter.FileLocations.AppLogsFolder + @"\ThwargLauncher-%PID%_log.txt";
            filepath = MagFilter.FileLocations.ExpandFilepath(filepath);
            MagFilter.FileLocations.CreateAnyNeededFolders(filepath);
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
            _viewModel.ShutSubsidiaryWindows();
            EndMonitoringGame();
        }
        private void EndMonitoringGame()
        {
            _uiGameMonitorBridge.Stop();
            _gameMonitor.Stop();
        }
        public static GameSession GetTheGameSessionByServerAccount(string serverName, string accountName)
        {
            // architectural problem getting to the game session map
            return theAppCoordinator._gameSessionMap.GetGameSessionByServerAccount(serverName: serverName, accountName: accountName);
        }
    }
}
