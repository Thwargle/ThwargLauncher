using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ThwargLauncher
{
    class BrowseServerViewModel
    {
        private readonly ObservableCollection<ServerModel> _serverModels = new ObservableCollection<ServerModel>();
        public ObservableCollection<ServerModel> AllServers { get { return _serverModels; } }
        public BrowseServerViewModel()
        {
            var persister = new GameManagement.ServerPersister(ServerManager.GetServerDataFolder());

            var availableServers = persister.GetWildWestServerList();
            // TODO - subtract out servers we have
            foreach (var servdata in availableServers)
            {
                ServerModel model = ServerModel.Create(servdata);
                _serverModels.Add(model);

            }
            //            _servers = new ObservableCollection<ServerModels>(persister.GetWildWestServerList());

        }
    }
}
