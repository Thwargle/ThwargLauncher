using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ThwargLauncher.GameManagement
{
    class PhatACServerLister
    {
        private const string EMU = "PhatAC";
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
                var phatServerItems = ReadPhatServerList(filepath);
                serverItemList.AddRange(phatServerItems);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Unable to read " + description + ": " + exc.ToString());
            }
        }
        private static bool StringToBool(string text, bool defval=false)
        {
            if (string.Compare(text, "true", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return true;
            }
            if (string.Compare(text, "false", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }
            if (text == "1") { return true; }
            if (text == "0") { return false; }
            return defval;
        }
        public static IList<Server.ServerItem> ReadPhatServerList(string filename)
        {
            var list = new List<Server.ServerItem>();
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
               
                var xmlDoc2 = new XmlDocument();
                xmlDoc2.Load(reader);
                foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                {
                    Server.ServerItem si = new Server.ServerItem();

                    si.ServerName = GetSubvalue(node, "name");
                    si.ServerDescription = GetSubvalue(node, "description");
                    si.ServerLoginEnabled = StringToBool(GetSubvalue(node, "enable_login"));
                    si.ServerIpAndPort = GetSubvalue(node, "connect_string");
                    si.EMU = EMU;
                    si.RodatSetting = GetSubvalue(node, "default_rodat");
                    list.Add(si);
                }
            }
            return list;
        }
        private static string GetSubvalue(XmlNode node, string key)
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
