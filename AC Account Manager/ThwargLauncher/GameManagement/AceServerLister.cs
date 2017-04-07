using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher.GameManagement
{
    class AceServerLister
    {
        private const string EMU = "ACE";
        private string _folder;

        public AceServerLister(string folder)
        {
            _folder = folder;
        }
        public IList<ServerInfo> LoadACEServers()
        {
            return LoadServers();
        }
        public IList<ServerInfo> LoadServers()
        {
            IList<ServerInfo> serverItemList = new List<ServerInfo>();
            try
            {
                string filepath = GetFilePath(ServerManager.AceServerList);
                // One-time migration from executable directory
                if (!File.Exists(filepath))
                {
                    if (File.Exists(ServerManager.AceServerList))
                    {
                        File.Copy(ServerManager.AceServerList, filepath);
                    }
                }
                if (!File.Exists(filepath))
                {
                    return serverItemList;
                }
                serverItemList = ServerPersister.ReadServerList(EMU, filepath);
            }
            catch(Exception exc)
            {
                Logger.WriteInfo("Unable to read ACE Server list xml: " + exc.ToString());
            }
            return serverItemList;
        }
        private string GetSubvalue(XmlNode node, string key)
        {
            var childNodes = node.SelectNodes(key);
            if (childNodes.Count == 0) { throw new Exception("Server lacked key: " + key); }
            var childNode = childNodes[0];
            string value = childNode.InnerText;
            return value;
        }
        private string GetFilePath(string filename)
        {
            string filepath = Path.Combine(_folder, filename);
            return filepath;
        }
    }
}
