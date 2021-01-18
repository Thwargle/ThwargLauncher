using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PublishedServerInfoMap = System.Collections.Generic.Dictionary<string, ThwargLauncher.GameManagement.ServerPersister.PublishedServerLocalInfo>;

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
            public string ServerAlias;
            public string ServerDesc;
            public string ConnectionString;
            public string GameApiUrl;
            public string LoginServerUrl;
            public string DiscordUrl;
            public string WebsiteUrl;
            public ServerModel.ServerEmuEnum EMU;
            public ServerModel.RodatEnum RodatSetting;
            public ServerModel.SecureEnum SecureSetting;
            public ServerModel.VisibilityEnum VisibilitySetting;
            public ServerModel.ServerSourceEnum ServerSource;
        }
        public class PublishedServerLocalInfo
        {
            public string Name;
            public string Alias;
            public Guid Id;
            public ServerModel.VisibilityEnum VisibilitySetting;
        }
        private const string PublishedGDLServerListFilename = "PublishedGDLServerList.xml";
        private const string PublishedACEServerListFilename = "PublishedACEACServerList.xml";
        private const string PublishedDFServerListFilename = "PublishedDFACServerList.xml";
        private const string PublishedWildWestServerListFilename = "PublishedWildWestServerList.xml";
        private const string LocalPublishedGDLServerInfosFilename = "PublishedServerInfo.xml";
        private const string UserServerListFilename = "UserServerList.xml";
        private string _serverDataFolder;
        private string _publishedGDLServersFilepath;
        private string _publishedACEServersFilepath;
        private string _publishedDFServersFilepath;
        private string _publishedWildWestServersFilepath;
        private string _localPublishedGDLServersInfoFilepath;
        private string _userServersFilepath;

        public ServerPersister(string serverDataFolder)
        {
            _serverDataFolder = serverDataFolder;
            _publishedGDLServersFilepath = Path.Combine(_serverDataFolder, PublishedGDLServerListFilename);
            _publishedACEServersFilepath = Path.Combine(_serverDataFolder, PublishedACEServerListFilename);
            _publishedDFServersFilepath = Path.Combine(_serverDataFolder, PublishedDFServerListFilename);
            _publishedWildWestServersFilepath = Path.Combine(_serverDataFolder, PublishedWildWestServerListFilename);
            _localPublishedGDLServersInfoFilepath = Path.Combine(_serverDataFolder, LocalPublishedGDLServerInfosFilename);
            _userServersFilepath = Path.Combine(_serverDataFolder, UserServerListFilename);
        }

        internal IEnumerable<ServerData> ReadUserServers()
        {
            string filepath = _userServersFilepath;
            ServerModel.ServerEmuEnum emu = ServerModel.ServerEmuEnum.GDLE;
            var servers = ReadServerList(ServerModel.ServerSourceEnum.User, emu, filepath);
            return servers;
        }
        private XElement CreateServerXmlElement(ServerModel server)
        {
            var xelem = new XElement("ServerItem",
                            new XElement("id", server.ServerId),
                            new XElement("name", server.ServerName),
                            new XElement("alias", server.ServerAlias),
                            new XElement("description", server.ServerDescription),
                            new XElement("emu", server.EMU),
                            new XElement("connect_string", server.ServerIpAndPort),
                            new XElement("discord_url", server.DiscordUrl),
                            new XElement("website_url", server.WebsiteUrl),
                            new XElement("GameApiUrlKey", server.GameApiUrl),
                            new XElement("LoginServerUrlKey", server.LoginServerUrl),
                            new XElement("enable_login", "true"),
                            new XElement("emu", server.EMU),
                            new XElement("default_rodat", server.RodatSetting),
                            new XElement("default_secure", server.SecureSetting),
                            new XElement("visibility", server.VisibilitySetting)
                            );
            return xelem;
        }

        private IEnumerable<ServerData> ReadServerList(ServerModel.ServerSourceEnum source, ServerModel.ServerEmuEnum emudef, string filepath)
        {
            var list = new List<ServerData>();
            if (File.Exists(filepath))
            {
                try
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
                            si.ServerAlias = GetOptionalSubvalue(node, "alias", null);
                            si.ServerDesc = GetSubvalue(node, "description");

                            si.ConnectionString = GetOptionalSubvalue(node, "connect_string", null);
                            if (si.ConnectionString == null)
                            {
                                var host = GetSubvalue(node, "server_host");
                                var port = GetSubvalue(node, "server_port");
                                si.ConnectionString = string.Format("{0}:{1}", host, port);
                            }
                            si.GameApiUrl = GetOptionalSubvalue(node, "GameApiUrlKey", "");
                            si.LoginServerUrl = GetOptionalSubvalue(node, "LoginServerUrlKey", "");
                            si.DiscordUrl = GetOptionalSubvalue(node, "DiscordUrl", "");
                            if (string.IsNullOrEmpty(si.DiscordUrl))
                            {
                                si.DiscordUrl = GetOptionalSubvalue(node, "discord_url", "");
                            }
                            si.WebsiteUrl = GetOptionalSubvalue(node, "website_url", "");
                            
                            string emustr = GetOptionalSubvalue(node, "emu", emudef.ToString());
                            si.EMU = ParseEmu(emustr, emudef);
                            si.ServerSource = source;
                            string rodatstr = GetOptionalSubvalue(node, "default_rodat", "");
                            si.RodatSetting = ParseRodat(rodatstr, defval: ServerModel.RodatEnum.Off);
                            string securestr = GetOptionalSubvalue(node, "default_secure", "false");
                            si.SecureSetting = ParseSecure(securestr, defval: ServerModel.SecureEnum.Off);
                            string visibilitystr = GetOptionalSubvalue(node, "visibility", "Visible");
                            si.VisibilitySetting = ParseVisibility(visibilitystr, defval: ServerModel.VisibilityEnum.Visible);
                            list.Add(si);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.WriteInfo("Unable to parse Published Server List: " + exc.ToString());
                }

            }
            return list;
        }
        private IEnumerable<ServerData> ReadPublishedGDLServerList(PublishedServerInfoMap publishedInfos)
        {
            var list = new List<ServerData>();
            string filepath = _publishedGDLServersFilepath;
            if (File.Exists(filepath))
            {
                try
                {
                    using (XmlTextReader reader = new XmlTextReader(filepath))
                    {

                        var xmlDoc2 = new XmlDocument();
                        xmlDoc2.Load(reader);
                        foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                        {
                            ServerData si = new ServerData();

                            si.ServerName = GetSubvalue(node, "name");
                            PublishedServerLocalInfo info = null;
                            if (!publishedInfos.ContainsKey(si.ServerName))
                            {
                                info = new PublishedServerLocalInfo();
                                info.Name = si.ServerName;
                                info.Id = Guid.NewGuid();
                                info.VisibilitySetting = ServerModel.VisibilityEnum.Visible;
                                info.Alias = null;
                                publishedInfos[si.ServerName] = info;
                            }
                            else
                            {
                                info = publishedInfos[si.ServerName];
                            }
                            si.ServerId = info.Id;
                            si.ServerAlias = info.Alias;
                            si.ServerDesc = GetSubvalue(node, "description");
                            si.DiscordUrl = GetSubvalue(node, "DiscordUrl");
                            si.ConnectionString = GetSubvalue(node, "connect_string");
                            si.EMU = ServerModel.ServerEmuEnum.GDLE;
                            si.ServerSource = ServerModel.ServerSourceEnum.Published;
                            string rodatstr = GetSubvalue(node, "default_rodat");
                            si.RodatSetting = ParseRodat(rodatstr, defval: ServerModel.RodatEnum.Off);
                            si.VisibilitySetting = info.VisibilitySetting;

                            list.Add(si);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.WriteInfo("Unable to parse Published GDL Server List: " + exc.ToString());
                }


            }
            return list;
        }

        private IEnumerable<ServerData> ReadPublishedACEServerList(PublishedServerInfoMap publishedInfos)
        {
            var list = new List<ServerData>();
            string filepath = _publishedACEServersFilepath;
            if (File.Exists(filepath))
            {
                try
                {
                    using (XmlTextReader reader = new XmlTextReader(filepath))
                    {

                        var xmlDoc2 = new XmlDocument();
                        xmlDoc2.Load(reader);
                        foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                        {
                            ServerData si = new ServerData();

                            si.ServerName = GetSubvalue(node, "name");
                            PublishedServerLocalInfo info = null;
                            if (!publishedInfos.ContainsKey(si.ServerName))
                            {
                                info = new PublishedServerLocalInfo();
                                info.Name = si.ServerName;
                                info.Id = Guid.NewGuid();
                                info.VisibilitySetting = ServerModel.VisibilityEnum.Visible;
                                info.Alias = null;
                                publishedInfos[si.ServerName] = info;
                            }
                            else
                            {
                                info = publishedInfos[si.ServerName];
                            }
                            si.ServerId = info.Id;
                            si.ServerAlias = info.Alias;
                            si.ServerDesc = GetSubvalue(node, "description");
                            si.DiscordUrl = GetSubvalue(node, "DiscordUrl");
                            si.ConnectionString = GetSubvalue(node, "connect_string");
                            si.EMU = ServerModel.ServerEmuEnum.ACE;
                            si.ServerSource = ServerModel.ServerSourceEnum.Published;
                            string rodatstr = GetSubvalue(node, "default_rodat");
                            si.RodatSetting = ParseRodat(rodatstr, defval: ServerModel.RodatEnum.Off);
                            si.VisibilitySetting = info.VisibilitySetting;

                            list.Add(si);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.WriteInfo("Unable to parse Published ACE Server List: " + exc.ToString());
                }
            }
            return list;
        }

        public IEnumerable<ServerData> GetPublishedGDLServerList()
        {
            var publishedServerInfos = LoadPublishedServerInfos();

            DownloadPublishedGDLServersToCacheIfPossible();
            var publishedServers = ReadPublishedGDLServerList(publishedServerInfos);

            SavePublishedServerInfos(publishedServerInfos);

            return publishedServers;
        }

        public IEnumerable<ServerData> GetPublishedACEServerList()
        {
            var publishedServerInfos = LoadPublishedServerInfos();

            DownloadPublishedACEServersToCacheIfPossible();
            var publishedServers = ReadPublishedACEServerList(publishedServerInfos);

            SavePublishedServerInfos(publishedServerInfos);

            return publishedServers;
        }

        public IEnumerable<ServerData> GetWildWestServerList()
        {
            DownloadWildWestServersToCacheIfPossible();
            var wildWestServers = ReadServerList(ServerModel.ServerSourceEnum.User, ServerModel.ServerEmuEnum.GDLE, _publishedWildWestServersFilepath);
            return wildWestServers;
        }

        private PublishedServerInfoMap LoadPublishedServerInfos()
        {
            string serverFolder = Path.GetDirectoryName(_userServersFilepath);
            CleanupObsoleteFiles(serverFolder); // TODO - get rid of this later

            var publishedServerInfos = new PublishedServerInfoMap();
            string filepath = _localPublishedGDLServersInfoFilepath;
            if (File.Exists(filepath))
            {
                try
                {
                    using (XmlTextReader reader = new XmlTextReader(filepath))
                    {
                        var xmlDoc2 = new XmlDocument();
                        xmlDoc2.Load(reader);
                        foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                        {
                            var info = new PublishedServerLocalInfo();
                            info.Name = GetSubvalue(node, "name");
                            Guid guid = StringToGuid(GetSubvalue(node, "id"));
                            if (guid == Guid.Empty) { guid = Guid.NewGuid(); }
                            info.Id = guid;
                            string visibilitystr = GetOptionalSubvalue(node, "visibility", "Visible"); // optional for upgrade by developers
                            info.VisibilitySetting = ParseVisibility(visibilitystr, ServerModel.VisibilityEnum.Visible);
                            info.Alias = GetOptionalSubvalue(node, "alias", null);
                            publishedServerInfos[info.Name] = info;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.WriteInfo("Unable to parse Published GDL Server List: " + exc.ToString());
                }
            }
            return publishedServerInfos;
        }
        private void SavePublishedServerInfos(PublishedServerInfoMap publishedServerInfos)
        {
            XElement root = new XElement("ArrayOfServerItem");
            XDocument doc = new XDocument(root);
            foreach (var item in publishedServerInfos)
            {
                string name = item.Key;
                var info = item.Value;
                string alias = info.Alias;
                if (string.IsNullOrEmpty(name)) { continue; }
                var xelem = new XElement("ServerItem",
                                new XElement("id", info.Id),
                                new XElement("name", name),
                                new XElement("alias", info.Alias),
                                new XElement("visibility", info.VisibilitySetting));
                root.Add(xelem);
            }
            doc.Save(_localPublishedGDLServersInfoFilepath);
        }
        private void DownloadPublishedGDLServersToCacheIfPossible()
        {
            try
            {
                string filepath = _publishedGDLServersFilepath;
                var url = Properties.Settings.Default.GDLServerListUrl;
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
                Logger.WriteInfo("Unable to download Published GDL Server List: " + exc.ToString());
            }
        }

        private void DownloadPublishedACEServersToCacheIfPossible()
        {
            try
            {
                string filepath = _publishedACEServersFilepath;
                var url = Properties.Settings.Default.ACEServerListUrl;
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
                Logger.WriteInfo("Unable to download Published ACE Server List: " + exc.ToString());
            }
        }

        private void DownloadACEServersToCacheIfPossible()
        {
            try
            {
                string filepath = _publishedACEServersFilepath;
                var url = Properties.Settings.Default.ACEServerListUrl;
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
                Logger.WriteInfo("Unable to download Published ACE Server List: " + exc.ToString());
            }
        }

        private void DownloadDFServersToCacheIfPossible()
        {
            try
            {
                string filepath = _publishedDFServersFilepath;
                var url = Properties.Settings.Default.DFServerListUrl;
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
                Logger.WriteInfo("Unable to download Published DF Server List: " + exc.ToString());
            }
        }

        private void DownloadWildWestServersToCacheIfPossible()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                string filepath = _publishedWildWestServersFilepath;
                var url = Properties.Settings.Default.WildWestServerListUrl;
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
                Logger.WriteInfo("Unable to download Published Wild West Server List: " + exc.ToString());
            }
        }

        public void WriteServerListToFile(IEnumerable<ServerModel> servers)
        {
            // Save all user servers
            var xdoc = WriteServersToXml(servers);
            WriteServerXmlToFile(xdoc, _userServersFilepath);

            // Save local published server info in case any visibilities changed
            var publishedServerInfos = LoadPublishedServerInfos();
            bool changed = false;
            // Update publishedServerInfos visibility data from in-memory data
            foreach (var info in publishedServerInfos.Values)
            {
                var server = ServerManager.ServerList.Where(s => s.ServerId == info.Id).FirstOrDefault();
                if (server != null)
                {
                    if (info.VisibilitySetting != server.VisibilitySetting)
                    {
                        info.VisibilitySetting = server.VisibilitySetting;
                        changed = true;
                    }
                    if (info.Alias != server.ServerAlias)
                    {
                        info.Alias = server.ServerAlias;
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                SavePublishedServerInfos(publishedServerInfos);
            }
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
            CleanupFile(folder, "GDLServerList.xml");
            CleanupFile(folder, "PublishedGDLServerList");
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
        private ServerModel.RodatEnum ParseRodat(string text, ServerModel.RodatEnum defval)
        {
            if (string.Compare(text, "On", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return ServerModel.RodatEnum.On;
            }
            else if (string.Compare(text, "Off", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return ServerModel.RodatEnum.Off;
            }
            else
            {
                return defval;
            }
        }
        private ServerModel.SecureEnum ParseSecure(string text, ServerModel.SecureEnum defval)
        {
            if (string.Compare(text, "On", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return ServerModel.SecureEnum.On;
            }
            else if (string.Compare(text, "Off", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return ServerModel.SecureEnum.Off;
            }
            else
            {
                return defval;
            }
        }
        private ServerModel.VisibilityEnum ParseVisibility(string text, ServerModel.VisibilityEnum defval)
        {
            if (string.Compare(text, "Invisible", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return ServerModel.VisibilityEnum.Invisible;
            }
            else if (string.Compare(text, "Visible", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return ServerModel.VisibilityEnum.Visible;
            }
            else
            {
                return defval;
            }
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
