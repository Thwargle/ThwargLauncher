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
            BeginMonitoringGame();

            ShowMainWindow();
        }
        private void ShowMainWindow()
        {
            var mainWindow = new MainWindow(_viewModel, _gameSessionMap, _gameMonitor);
            mainWindow.Closing += mainWindow_Closing;
            mainWindow.Show();
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
            _uiGameMonitorBridge = new UiGameMonitorBridge(_gameMonitor, _viewModel);
            _uiGameMonitorBridge.Start();
            _gameMonitor.Start();
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
            EndMonitoringGame();
        }
        private void EndMonitoringGame()
        {
            _uiGameMonitorBridge.Stop();
            _gameMonitor.Stop();
        }
    }
}
