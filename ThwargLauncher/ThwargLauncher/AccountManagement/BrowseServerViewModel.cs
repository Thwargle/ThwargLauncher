using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ThwargLauncher
{
    class BrowseServerViewModel
    {
        private readonly ObservableCollection<ServerModel> _serverModels = new ObservableCollection<ServerModel>();
        public ObservableCollection<ServerModel> AllServers { get { return _serverModels; } }
        ICommand _ImportCommand;
        public BrowseServerViewModel()
        {
            LoadServers();
        }
        private void LoadServers()
        {
            _serverModels.Clear();
            var persister = new GameManagement.ServerPersister(ServerManager.GetServerDataFolder());

            var allServers = persister.GetWildWestServerList();
            var availableServers = allServers.Where(q => !IsInOurServers(q));
            foreach (var servdata in availableServers)
            {
                ServerModel model = ServerModel.Create(servdata);
                _serverModels.Add(model);
            }

        }
        private bool IsInOurServers(GameManagement.ServerPersister.ServerData srvdata)
        {
            var result = ServerManager.ServerList.FirstOrDefault(z => z.IsEqual(srvdata));
            return (result != null);
        }
        private bool IsModelInOurServers(ServerModel server)
        {
            return ServerManager.ServerList.Contains(server);
        }
        public ICommand ImportCommand
        {
            get
            {
                if (_ImportCommand == null)
                {
                    _ImportCommand = new ParameterizedDelegateCommand(CanImport, Import);
                }
                return _ImportCommand;
            }
        }
        private void Import(object parameter)
        {
            ServerModel server = parameter as ServerModel;
            if (IsModelInOurServers(server)) { return; }
            ServerManager.ServerList.Add(server);
            LoadServers();
        }
        private bool CanImport(object paraamter)
        {
            return true;
        }


        class ParameterizedDelegateCommand : ICommand
        {
            Predicate<object> _canExecute;
            Action<object> _doExecute;
            public ParameterizedDelegateCommand(Predicate<object> canexecute, Action<object> execute)
           : this()
            {
                _canExecute = canexecute;
                _doExecute = execute;
            }
            public ParameterizedDelegateCommand()
            { }
            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute(parameter);
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                _doExecute(parameter);
            }
        }
    }
}
