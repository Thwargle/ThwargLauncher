using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ThwargLauncher.GameManagement
{
    class PhatACServerLister
    {
        

        public List<Server.ServerItem> loadPhatServers()
        {
            return loadServers("PhatAC");
        }

        public List<Server.ServerItem> loadServers(string EMU)
        {
            var m_strFilePath = "https://raw.githubusercontent.com/cmoski/pac_launcher_config/master/servers_v2.xml";
            string xmlStr;
            using (var wc = new WebClient())
            {
                xmlStr = wc.DownloadString(m_strFilePath);
            }
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlStr);

            List<Server.ServerItem> serverItemList = new List<Server.ServerItem>();

            foreach(XmlNode node in xmlDoc.SelectNodes("//ServerItem"))
            {
                Server.ServerItem si = new Server.ServerItem();

                si.ServerName = GetSubvalue(node, "name");
                si.ServerIP = GetSubvalue(node, "connect_string");
                si.EMU = EMU;
                serverItemList.Add(si);
            }

            XmlTextReader reader = new XmlTextReader("PhatACServerList.xml");
            var xmlDoc2 = new XmlDocument();
            xmlDoc.Load(reader);
            foreach (XmlNode node in xmlDoc2.SelectNodes("//ServerItem"))
            {
                Server.ServerItem si2 = new Server.ServerItem();

                si2.ServerName = GetSubvalue(node, "name");
                si2.ServerIP = GetSubvalue(node, "connect_string");
                si2.EMU = EMU;
                serverItemList.Add(si2);
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
