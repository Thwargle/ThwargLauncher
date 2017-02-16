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
        public List<Server.ServerItem> loadACEServers()
        {
            return loadServers("ACE");
        }
        public List<Server.ServerItem> loadServers(string EMU)
        {

            XmlTextReader reader = new XmlTextReader("ACEServerList.xml");
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);

            List<Server.ServerItem> serverItemList = new List<Server.ServerItem>();
            foreach (XmlNode node in xmlDoc.SelectNodes("//ServerItem"))
            {
                Server.ServerItem si = new Server.ServerItem();

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
