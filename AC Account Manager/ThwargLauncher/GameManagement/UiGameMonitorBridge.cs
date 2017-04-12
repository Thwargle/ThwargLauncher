using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace ThwargLauncher
{
    class UiGameMonitorBridge
    {
        private GameMonitor _gameMonitor;
        private MainWindowViewModel _viewModel;
        private SynchronizationContext _uicontext;

        public UiGameMonitorBridge(GameMonitor gameMonitor, MainWindowViewModel viewModel)
        {
            if (gameMonitor == null) { throw new Exception("Null GameMonitor in UiGameMonitorBridge()"); }
            if (viewModel == null) { throw new Exception("Null MainWindowViewModel in UiGameMonitorBridge()"); }

            _gameMonitor = gameMonitor;
            _viewModel = viewModel;
            _uicontext = SynchronizationContext.Current;
        }
        public void Start()
        {
            _gameMonitor.GameChangeEvent += _gameMonitor_GameChangeEvent;
        }
        public void Stop()
        {
            _gameMonitor.GameChangeEvent -= _gameMonitor_GameChangeEvent;
        }
        /// <summary>
        /// Handle events from Game Monitor, on monitor thread
        /// We just dispatch them asynchronously over to ui thread
        /// </summary>
        private void _gameMonitor_GameChangeEvent(GameSession gameSession, GameMonitor.GameChangeType changeType)
        {
            object state = null;
            _uicontext.Post(new SendOrPostCallback(
                (obj) => UiHandleGameChangeEvent(gameSession, changeType)), state);
        }
        /// <summary>
        /// Handle Game Monitor events, now on ui thread
        /// </summary>
        private void UiHandleGameChangeEvent(GameSession gameSession, GameMonitor.GameChangeType changeType)
        {
            Logger.WriteDebug("Game {0} status change type {1} status {2}", gameSession.Description, changeType, gameSession.Status);
            _viewModel.UpdateAccountStatus(gameSession.ServerName, gameSession.AccountName, gameSession.Status);
        }
        /// <summary>
        /// Handle events from Game Monitor, on monitor thread
        /// We just dispatch them asynchronously over to ui thread
        /// </summary>
        private void _gameMonitor_GameCommandEvent(GameSession gameSession, string command)
        {
            object state = null;
            _uicontext.Post(new SendOrPostCallback(
                (obj) => UiHandleGameChangeEvent(gameSession, command)), state);
        }
        /// <summary>
        /// Handle Game Command events, now on ui thread
        /// </summary>
        private void UiHandleGameChangeEvent(GameSession gameSession, string command)
        {
            _viewModel.ExecuteGameCommand(gameSession.ServerName, gameSession.AccountName, command);
        }
    }
}
