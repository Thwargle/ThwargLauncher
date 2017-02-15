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
        public class ServerItem
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }

        }
        public List<ServerItem> loadServers()
        {
            var m_strFilePath = "https://raw.githubusercontent.com/cmoski/pac_launcher_config/master/servers_v2.xml";
            string xmlStr;
            using (var wc = new WebClient())
            {
                xmlStr = wc.DownloadString(m_strFilePath);
            }
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlStr);

            List<ServerItem> serverItemList = new List<ServerItem>();

            foreach(XmlNode node in xmlDoc.SelectNodes("//ServerItem"))
            {
                ServerItem si = new ServerItem();

                si.ServerName = GetSubvalue(node, "name");
                si.ServerIP = GetSubvalue(node, "connect_string");
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
