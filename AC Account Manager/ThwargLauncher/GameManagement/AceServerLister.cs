using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher.GameManagement
{
    class AceServerLister
    {
        public class ServerItem
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }
            public string EMU { get; set; }

        }

        public List<ServerItem> loadACEServers()
        {
            return loadServers("ACE");
        }
        public List<ServerItem> loadServers(string EMU)
        {

            XmlTextReader reader = new XmlTextReader("ACEServerList.xml");
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);

            List<ServerItem> serverItemList = new List<ServerItem>();
            foreach (XmlNode node in xmlDoc.SelectNodes("//ServerItem"))
            {
                ServerItem si = new ServerItem();

                si.ServerName = GetSubvalue(node, "name");
                si.ServerIP = GetSubvalue(node, "connect_string");
                si.EMU = EMU;
                serverItemList.Add(si);
            }

            return serverItemList;

        }
        private string GetSubvalue(XmlNode node, string key)
        {
            var childNodes = node.SelectNodes(key);
            if (childNodes.Count == 0) { throw new Exception("Server lacked name"); }
            var childNode = childNodes[0];
            string value = childNode.InnerText;
            return value;
        }
    }
}
