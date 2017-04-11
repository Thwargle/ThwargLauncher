using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThwargLauncher.UtilityCode;

namespace ThwargLauncher
{
    public class ServerManager
    {
        public static List<ServerModel> ServerList = new List<ServerModel>();
        public static bool IsLoaded;


        public static void LoadServerLists()
        {
            string folder = GetServerDataFolder();
            var persister = new GameManagement.ServerPersister(folder);
            var publishedPhatServers = persister.GetPublishedPhatServerList();
            var userServers = persister.ReadUserServers();

            var servers = new List<GameManagement.ServerPersister.ServerData>();
            servers.AddRange(publishedPhatServers);
            servers.AddRange(userServers);
            var distinctServers = servers.Distinct().ToList();
            foreach (var sdata in distinctServers)
            {
                AddOrUpdateServer(sdata);

            }
            IsLoaded = true;
        }
        private static string GetServerDataFolder()
        {
            string folderpath = System.IO.Path.Combine(MagFilter.FileLocations.AppFolder, "Servers");
            folderpath = MagFilter.FileLocations.ExpandFilepath(folderpath);
            MagFilter.FileLocations.CreateAnyNeededFoldersOfFolder(folderpath);
            return folderpath;
        }
        private static void AddOrUpdateServer(GameManagement.ServerPersister.ServerData servdata)
        {
            var existing = ServerList.FirstOrDefault(s => s.IsEqual(servdata));
            if (existing != null)
            {
                existing.ServerName = servdata.ServerName;
                existing.ServerDescription = servdata.ServerDesc;
                existing.ServerIpAndPort = servdata.ConnectionString;
                existing.RodatSetting = servdata.RodatSetting;
                existing.VisibilitySetting = servdata.VisibilitySetting;
                existing.EMU = servdata.EMU;
            }
            else
            {
                ServerModel model = ServerModel.Create(servdata);
                ServerList.Add(model);
            }
        }
        internal static void AddNewServer(GameManagement.ServerPersister.ServerData servdata)
        {
            AddOrUpdateServer(servdata);
            SaveServerListToDisk();
        }
        internal static void SaveServerListToDisk()
        {
            var userServers = ServerList.Where(s => s.ServerSource != ServerModel.ServerSourceEnum.Published);

            var persister = new GameManagement.ServerPersister(GetServerDataFolder());
            persister.WriteServerListToFile(userServers);
        }
    }
}
