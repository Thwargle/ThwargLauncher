using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ThwargLauncher.GameManagement
{
    class ServerPersister
    {
        public class EditedServerInfo
        {
            public string ServerName;
            public string ServerDesc;
            public string ConnectionString;
            public string RodatSetting;
        }
        public static void AddNewServerToXmlDoc(EditedServerInfo server, XDocument doc)
        {
            XElement serverItemArray = doc.Element("ArrayOfServerItem");
            serverItemArray.Add(new XElement("ServerItem",
                            new XElement("name", server.ServerName),
                            new XElement("description", server.ServerDesc),
                            new XElement("connect_string", server.ConnectionString),
                            new XElement("enable_login", "true"),
                            new XElement("custom_credentials", "true"),
                            new XElement("default_rodat", server.RodatSetting),
                            new XElement("default_username", "username"),
                            new XElement("default_password", "password"),
                            new XElement("allow_dual_log", "true"))
                    );
        }

        public static IList<ServerModel> ReadServerList(string emu, ServerModel.ServerSourceEnum source, string filename)
        {
            var list = new List<ServerModel>();
            using (XmlTextReader reader = new XmlTextReader(filename))
            {

                var xmlDoc2 = new XmlDocument();
                xmlDoc2.Load(reader);
                foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
                {
                    ServerModel si = new ServerModel();

                    si.ServerName = GetSubvalue(node, "name");
                    si.ServerDescription = GetSubvalue(node, "description");
                    si.ServerLoginEnabled = StringToBool(GetSubvalue(node, "enable_login"));
                    si.ServerIpAndPort = GetSubvalue(node, "connect_string");
                    si.EMU = emu;
                    si.ServerSource = source;
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
