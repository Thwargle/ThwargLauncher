using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThwargLauncher.UtilityCode;

namespace ThwargLauncher
{
    public class ServerManager
    {
        public static List<Server.ServerItem> ServerList = new List<Server.ServerItem>();
        public static bool IsLoaded;

        public void LoadServerLists()
        {
            var phatServers = (new GameManagement.PhatACServerLister()).loadPhatServers();
            var aceServers = (new GameManagement.AceServerLister()).loadACEServers();
            foreach (var serverItem in phatServers.DistinctBy(p => p.GetHashCode()))
            {
                AddOrUpdateServer(serverItem);
            }
            foreach (var serverItem in aceServers.DistinctBy(p => p.GetHashCode()))
            {
                AddOrUpdateServer(serverItem);
            }
            IsLoaded = true;
        }
        private void AddOrUpdateServer(Server.ServerItem server)
        {
            var existing = ServerList.FirstOrDefault(s => s.GetHashCode() == server.GetHashCode());
            if (existing != null)
            {
                // Currently we don't update because our GUI doesn't support editing existing servers
            }
            else
            {
                ServerList.Add(server);
            }
        }
    }
}
