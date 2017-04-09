using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CommonControls;

namespace ThwargLauncher
{
    class EditServersViewModel
    {
        public ObservableCollection<ServerModel> ServerList{ get; set; }
        public ICommand AddServerCommand { get; private set; }
        public Action CloseAction { get; set; }
        public bool AddServerRequested;

        public EditServersViewModel()
        {
            ServerList = new ObservableCollection<ServerModel>();
            foreach (var server in ServerManager.ServerList) // .Where(s => s.ServerSource != ServerModel.ServerSourceEnum.Published ))
            {
                ServerList.Add(server);
            }
            AddServerCommand = new DelegateCommand(AddNewServer);
        }
        private void AddNewServer()
        {
            AddServerRequested = true;
            if (CloseAction != null) { CloseAction(); }
        }
    }
}
