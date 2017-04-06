using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
//using System.Threading.Tasks;
using System.Xml;

namespace ThwargLauncher.GameManagement
{
    class PhatACServerLister
    {
        private string _folder;

        public PhatACServerLister(string folder)
        {
            _folder = folder;
        }
        public List<Server.ServerItem> LoadPhatServers()
        {
            return LoadServers();
        }
        private List<Server.ServerItem> LoadServers()
        {
            List<Server.ServerItem> serverItemList = new List<Server.ServerItem>();
            DownloadPublishedPhatServers();
            AddPhatServersFromFile(serverItemList, ServerManager.PublishedPhatServerList, "Published Phat Server List");
            AddPhatServersFromFile(serverItemList, ServerManager.PhatServerList, "User's Phat Server List");
            return serverItemList;
        }
        private void DownloadPublishedPhatServers()
        {
            try
            {
                var m_strFilePath = Properties.Settings.Default.PhatServerListUrl;
                string xmlStr;
                using (var wc = new WebClient())
                {
                    xmlStr = wc.DownloadString(m_strFilePath);
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlStr);
                string filepath = GetFilePath(ServerManager.PublishedPhatServerList);
                xmlDoc.Save(filepath);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Unable to download Published Phat Server List: " + exc.ToString());
            }
        }
        private void AddPhatServersFromFile(List<Server.ServerItem> serverItemList, string filename, string description)
        {
            try
            {
                string filepath = GetFilePath(filename);
                // One-time migration from executable directory
                if (!File.Exists(filepath))
                {
                    if (File.Exists(filename))
                    {
                        File.Copy(filename, filepath);
                    }
                }
                if (!File.Exists(filepath))
                {
                    return;
                }
                const string EMU = "PhatAC";
                var phatServerItems = ServerPersister.ReadServerList(EMU, filepath);
                serverItemList.AddRange(phatServerItems);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Unable to read " + description + ": " + exc.ToString());
            }
        }
        private string GetFilePath(string filename)
        {
            string filepath = Path.Combine(_folder, filename);
            return filepath;
        }
    }
}
