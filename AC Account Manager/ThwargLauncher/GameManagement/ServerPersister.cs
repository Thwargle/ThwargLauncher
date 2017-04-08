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
            public string ServerName;
            public string ServerDesc;
            public string ConnectionString;
            public string EMU;
            public string RodatSetting;
            public ServerModel.ServerSourceEnum ServerSource;
            public bool LoginEnabled; // TODO - what is this?
        }

        internal IEnumerable<ServerData> ReadUserServers(string filepath)
        {
            var servers = ReadServerList(ServerModel.ServerSourceEnum.User, "?", filepath);
            return servers;
        }
        public static void AddNewServerToXmlDoc(ServerData server, XDocument doc)
        {
            XElement serverItemArray = doc.Element("ArrayOfServerItem");
            var newitem = CreateServerXmlElement(server);
            serverItemArray.Add(newitem);
        }
        public void AddNewServerToXmlDoc_unused(ServerModel server, XDocument doc)
        {
            XElement serverItemArray = doc.Element("ArrayOfServerItem");
            var newitem = CreateServerXmlElement(server);
            serverItemArray.Add(newitem);
        }
        private static XElement CreateServerXmlElement(ServerData server)
        {
            var xelem = new XElement("ServerItem",
                            new XElement("name", server.ServerName),
                            new XElement("description", server.ServerDesc),
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
                            new XElement("name", server.ServerName),
                            new XElement("description", server.ServerDescription),
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
        private IEnumerable<ServerData> ReadServerList(ServerModel.ServerSourceEnum source, string emu, string filepath)
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
                        si.ServerDesc = GetSubvalue(node, "description");
                        si.LoginEnabled = StringToBool(GetOptionalSubvalue(node, "enable_login", "true"));
                        si.ConnectionString = GetSubvalue(node, "connect_string");
                        si.EMU = GetOptionalSubvalue(node, "emu", emu);
                        si.ServerSource = source;
                        si.RodatSetting = GetSubvalue(node, "default_rodat");
                        list.Add(si);
                    }
                }
            }
            return list;
        }
        private IEnumerable<ServerData> ReadPublishedPhatServerList(string filepath)
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
                        si.ServerDesc = GetSubvalue(node, "description");
                        si.LoginEnabled = StringToBool(GetOptionalSubvalue(node, "enable_login", "true"));
                        si.ConnectionString = GetSubvalue(node, "connect_string");
                        si.EMU = "Phat";
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
            DownloadPublishedPhatServersToCacheIfPossible(filepath);
            var publishedServers = ReadPublishedPhatServerList(filepath);
            return publishedServers;
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
