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
        public ICommand BrowseServerCommand { get; private set; }
        public Action CloseAction { get; set; }
        public bool AddServerRequested;
        public bool BrowseServerRequested;

        public EditServersViewModel()
        {
            ServerList = new ObservableCollection<ServerModel>();
            foreach (var server in ServerManager.ServerList) // .Where(s => s.ServerSource != ServerModel.ServerSourceEnum.Published ))
            {
                ServerList.Add(server);
            }
            AddServerCommand = new DelegateCommand(AddNewServer);
            BrowseServerCommand = new DelegateCommand(BrowseNewServer);
            ServerList.CollectionChanged += ServerList_CollectionChanged;
        }
        void ServerList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var deletedServers = new List<ServerModel>();
            var idsToDelete = new Dictionary<Guid, int>();
            foreach (var item in e.OldItems)
            {
                var server = item as ServerModel;
                idsToDelete[server.ServerId] = 1;
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var server = item as ServerModel;
                    if (idsToDelete.ContainsKey(server.ServerId))
                    {
                        idsToDelete.Remove(server.ServerId);
                    }
                }
            }
            foreach (var id in idsToDelete.Keys)
            {
                ServerManager.DeleteServerById(id);
            }
        }
        private void AddNewServer()
        {
            AddServerRequested = true;
            if (CloseAction != null) { CloseAction(); }
        }
        private void BrowseNewServer()
        {
            BrowseServerRequested = true;
            if (CloseAction != null) { CloseAction(); }
        }
    }
}
