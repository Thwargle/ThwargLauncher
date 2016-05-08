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
        private void _gameMonitor_GameChangeEvent(GameMonitor.GameChangeType changeType, GameSession gameSession)
        {
            object state = null;
            _uicontext.Post(new SendOrPostCallback(
                (obj) => UiHandleGameChangeEvent(changeType, gameSession)), state);
        }
        /// <summary>
        /// Handle Game Monitor events, now on ui thread
        /// </summary>
        private void UiHandleGameChangeEvent(GameMonitor.GameChangeType changeType, GameSession gameSession)
        {
            _viewModel.updateAccountStatus(gameSession.Status, gameSession.ServerName, gameSession.AccountName);
        }
    }
}
