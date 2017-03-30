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
        private const string EMU = "ACE";

        public List<Server.ServerItem> loadACEServers()
        {
            return loadServers();
        }
        public List<Server.ServerItem> loadServers()
        {
            List<Server.ServerItem> serverItemList = new List<Server.ServerItem>();
            try
            {
                XmlTextReader reader = new XmlTextReader("ACEServerList.xml");
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);

                foreach (XmlNode node in xmlDoc.SelectNodes("//ServerItem"))
                {
                    Server.ServerItem si = new Server.ServerItem();

                    si.ServerName = GetSubvalue(node, "name");
                    si.ServerIpAndPort = GetSubvalue(node, "connect_string");
                    si.EMU = EMU;
                    serverItemList.Add(si);
                }
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
            if (childNodes.Count == 0) { throw new Exception("Server lacked name"); }
            var childNode = childNodes[0];
            string value = childNode.InnerText;
            return value;
        }
    }
}
