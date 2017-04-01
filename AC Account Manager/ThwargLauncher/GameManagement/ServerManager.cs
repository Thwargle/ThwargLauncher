using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThwargLauncher.UtilityCode;

namespace ThwargLauncher
{
    public class ServerManager
    {
        public const string PublishedPhatServerList = "PublishedPhatACServerList";
        public const string PhatServerList = "PhatACServerList.xml";
        public const string AceServerList = "ACEServerList.xml";
        public static List<Server.ServerItem> ServerList = new List<Server.ServerItem>();
        public static bool IsLoaded;

        public void LoadServerLists()
        {
            string folder = GetServerDataFolder();
            var phatLister = new GameManagement.PhatACServerLister(folder);
            var phatServers = phatLister.LoadPhatServers();
            foreach (var serverItem in phatServers.DistinctBy(p => p.GetHashCode()))
            {
                AddOrUpdateServer(serverItem);
            }
            var aceLister = new GameManagement.AceServerLister(folder);
            var aceServers = aceLister.LoadACEServers();
            foreach (var serverItem in aceServers.DistinctBy(p => p.GetHashCode()))
            {
                AddOrUpdateServer(serverItem);
            }
            IsLoaded = true;
        }
        public string GetServerDataFolder()
        {
            string folderpath = System.IO.Path.Combine(MagFilter.FileLocations.AppFolder, "Servers");
            folderpath = MagFilter.FileLocations.ExpandFilepath(folderpath);
            MagFilter.FileLocations.CreateAnyNeededFoldersOfFolder(folderpath);
            return folderpath;
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
