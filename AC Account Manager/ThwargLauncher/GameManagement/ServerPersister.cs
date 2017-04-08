using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ThwargLauncher.GameManagement
{
    internal class ServerPersister
    {
        /// <summary>
        /// This is raw data about a server, used for temporary storage when moving data around
        /// Or reading or writing data
        /// This is not master data
        /// </summary>
        public class ServerData
        {
            public Guid ServerId;
            public string ServerName;
            public string ServerDesc;
            public string ConnectionString;
            public ServerModel.ServerEmuEnum EMU;
            public string RodatSetting;
            public ServerModel.ServerSourceEnum ServerSource;
            public bool LoginEnabled; // TODO - what is this?
        }

        internal IEnumerable<ServerData> ReadUserServers(string filepath)
        {
            ServerModel.ServerEmuEnum emu = ServerModel.ServerEmuEnum.Phat;
            var servers = ReadServerList(ServerModel.ServerSourceEnum.User, emu, filepath);
            return servers;
        }
        private static XElement CreateServerXmlElement_unused(ServerData server)
        {
            var xelem = new XElement("ServerItem",
                            new XElement("id", server.ServerId),
                            new XElement("name", server.ServerName),
                            new XElement("description", server.ServerDesc),
                            new XElement("emu", server.EMU),
                            new XElement("connect_string", server.ConnectionString),
                            new XElement("enable_login", "true"),
                            new XElement("custom_credentials", "true"),
                            new XElement("default_rodat", server.RodatSetting),
                            new XElement("default_username", "username"),
                            new XElement("default_password", "password"),
                            new XElement("allow_dual_log", "true")
                            );
            return xelem;
        }
        private XElement CreateServerXmlElement(ServerModel server)
        {
            var xelem = new XElement("ServerItem",
                            new XElement("id", server.ServerId),
                            new XElement("name", server.ServerName),
                            new XElement("description", server.ServerDescription),
                            new XElement("emu", server.EMU),
                            new XElement("connect_string", server.ServerIpAndPort),
                            new XElement("enable_login", "true"),
                            new XElement("custom_credentials", "true"),
                            new XElement("emu", server.EMU),
                            new XElement("default_rodat", server.RodatSetting),
                            new XElement("default_username", "username"),
                            new XElement("default_password", "password"),
                            new XElement("allow_dual_log", "true")
                            );
            return xelem;
        }
        private IEnumerable<ServerData> ReadServerList(ServerModel.ServerSourceEnum source, ServerModel.ServerEmuEnum emudef, string filepath)
        {
            var list = new List<ServerData>();
            if (File.Exists(filepath))
            {
                using (XmlTextReader reader = new XmlTextReader(filepath))
                {

                    var xmlDoc2 = new XmlDocument();
                    xmlDoc2.Load(reader);
                    foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                    {
                        ServerData si = new ServerData();

                        Guid guid = StringToGuid(GetOptionalSubvalue(node, "id", ""));
                        if (guid == Guid.Empty)
                        {
                             guid = Guid.NewGuid(); // temporary compatibility step - to be removed
                        }
                        si.ServerId = guid;
                        si.ServerName = GetSubvalue(node, "name");
                        si.ServerDesc = GetSubvalue(node, "description");
                        si.LoginEnabled = StringToBool(GetOptionalSubvalue(node, "enable_login", "true"));
                        si.ConnectionString = GetSubvalue(node, "connect_string");
                        string emustr = GetOptionalSubvalue(node, "emu", emudef.ToString());
                        si.EMU = ParseEmu(emustr, emudef);
                        si.ServerSource = source;
                        si.RodatSetting = GetSubvalue(node, "default_rodat");
                        list.Add(si);
                    }
                }
            }
            return list;
        }
        private IEnumerable<ServerData> ReadPublishedPhatServerList(string filepath, Dictionary<string, Guid> publishedIds)
        {
            var list = new List<ServerData>();
            if (File.Exists(filepath))
            {
                using (XmlTextReader reader = new XmlTextReader(filepath))
                {

                    var xmlDoc2 = new XmlDocument();
                    xmlDoc2.Load(reader);
                    foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                    {
                        ServerData si = new ServerData();

                        si.ServerName = GetSubvalue(node, "name");
                        if (!publishedIds.ContainsKey(si.ServerName))
                        {
                            publishedIds[si.ServerName] = Guid.NewGuid();
                        }
                        si.ServerId = publishedIds[si.ServerName];
                        si.ServerDesc = GetSubvalue(node, "description");
                        si.LoginEnabled = StringToBool(GetOptionalSubvalue(node, "enable_login", "true"));
                        si.ConnectionString = GetSubvalue(node, "connect_string");
                        si.EMU = ServerModel.ServerEmuEnum.Phat;
                        si.ServerSource = ServerModel.ServerSourceEnum.Published;
                        si.RodatSetting = GetSubvalue(node, "default_rodat");

                        list.Add(si);
                    }
                }
            }
            return list;
        }

        public IEnumerable<ServerData> GetPublishedPhatServerList(string filepath)
        {
            string serverFolder = Path.GetDirectoryName(filepath);
            CleanupObsoleteFiles(serverFolder);
            string cachedIdsFilepath = Path.Combine(serverFolder, "PublishedServerIds.xml");
            var publishedServerIds = LoadPublishedServerIds(cachedIdsFilepath);

            DownloadPublishedPhatServersToCacheIfPossible(filepath);
            var publishedServers = ReadPublishedPhatServerList(filepath, publishedServerIds);

            SavePublishedServerIds(cachedIdsFilepath, publishedServerIds);

            return publishedServers;
        }
        private Dictionary<string, Guid> LoadPublishedServerIds(string filepath)
        {
            var publishedServerIds = new Dictionary<string, Guid>();
            if (File.Exists(filepath))
            {
                using (XmlTextReader reader = new XmlTextReader(filepath))
                {

                    var xmlDoc2 = new XmlDocument();
                    xmlDoc2.Load(reader);
                    foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                    {
                        string serverName = GetSubvalue(node, "name");
                        Guid guid = StringToGuid(GetSubvalue(node, "id"));
                        if (guid != Guid.Empty)
                        {
                            publishedServerIds[serverName] = guid;
                        }
                    }
                }
            }
            return publishedServerIds;
        }
        private void SavePublishedServerIds(string filepath, Dictionary<string, Guid> publishedServerIds)
        {
            XElement root = new XElement("ArrayOfServerItem");
            XDocument doc = new XDocument(root);
            foreach (var item in publishedServerIds)
            {
                string name = item.Key;
                Guid id = item.Value;
                if (string.IsNullOrEmpty(name)) { continue; }
                if (id == Guid.Empty) { continue; }
                var xelem = new XElement("ServerItem",
                                new XElement("id", id),
                                new XElement("name", name));
                root.Add(xelem);
            }
            doc.Save(filepath);
        }
        private void DownloadPublishedPhatServersToCacheIfPossible(string filepath)
        {
            try
            {
                var url = Properties.Settings.Default.PhatServerListUrl;
                string xmlStr;
                using (var wc = new WebClient())
                {
                    xmlStr = wc.DownloadString(url);
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlStr);
                xmlDoc.Save(filepath);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Unable to download Published Phat Server List: " + exc.ToString());
            }
        }
        public void WriteServerListToFile(IEnumerable<ServerModel> servers, string filepath)
        {
            var xdoc = WriteServersToXml(servers);
            WriteServerXmlToFile(xdoc, filepath);
        }
        /// <summary>
        /// Delete some files from earlier versions which are no longer used
        /// This can be removed in a few weeks
        /// - 2017-04-08
        /// </summary>
        /// <param name="folder"></param>
        private void CleanupObsoleteFiles(string folder)
        {
            CleanupFile(folder, "ACEServerList.xml");
            CleanupFile(folder, "PhatACServerList.xml");
            CleanupFile(folder, "PublishedPhatACServerList");
        }
        private void CleanupFile(string folder, string file)
        {
            string filepath = System.IO.Path.Combine(folder, file);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }
        private XDocument WriteServersToXml(IEnumerable<ServerModel> servers)
        {
            XElement root = new XElement("ArrayOfServerItem");
            XDocument doc = new XDocument(root);
            foreach (var server in servers)
            {
                if (server.ServerSource != ServerModel.ServerSourceEnum.Published)
                {
                    var xelem = CreateServerXmlElement(server);
                    root.Add(xelem);
                }
            }
            return doc;
        }
        private void WriteServerXmlToFile(XDocument xdoc, string filepath)
        {
            xdoc.Save(filepath);
        }
        private static string GetSubvalue(XmlNode node, string key)
        {
            var childNodes = node.SelectNodes(key);
            if (childNodes.Count == 0) { throw new Exception("Server lacked key: " + key); }
            var childNode = childNodes[0];
            string value = childNode.InnerText;
            return value;
        }
        private static string GetOptionalSubvalue(XmlNode node, string key, string defval)
        {
            var childNodes = node.SelectNodes(key);
            if (childNodes.Count == 0) { return defval; }
            var childNode = childNodes[0];
            string value = childNode.InnerText;
            return value;
        }
        private ServerModel.ServerEmuEnum ParseEmu(string text, ServerModel.ServerEmuEnum defval)
        {
            ServerModel.ServerEmuEnum value = defval;
            Enum.TryParse(text, out value);
            return value;
        }
        private static Guid StringToGuid(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Guid guid;
                if (Guid.TryParse(text, out guid))
                {
                    return guid;
                }
            }
            return Guid.Empty;
        }
        private static bool StringToBool(string text, bool defval = false)
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
    }
}
