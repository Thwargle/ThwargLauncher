using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using WindowPlacementUtil;

namespace ThwargLauncher.AccountManagement
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        string serverName;
        string serverDesc;
        string serverIP;
        string serverPort;
        string connectionString;
        public AddServer()
        {
            InitializeComponent();
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (rdACEServer.IsChecked.HasValue && rdACEServer.IsChecked.Value)
            {
                XDocument doc = XDocument.Load("ACEServerList.xml");
                createElement(doc);
                doc.Save("ACEServerList.xml");
            }
            else if(rdPhatACServer.IsChecked.HasValue && rdPhatACServer.IsChecked.Value)
            {
                XDocument doc = XDocument.Load("PhatACServerList.xml");
                createElement(doc);
                doc.Save("PhatACServerList.xml");
            }
            this.Close();
        }

        private void createElement(XDocument doc)
        {
            serverName = txtServerName.Text;
            serverDesc = txtServeDesc.Text;
            serverIP = txtServerIP.Text;
            serverPort = txtServerPort.Text;
            connectionString = serverIP + ":" + serverPort;

            XElement serverItemArray = doc.Element("ArrayOfServerItem");
            serverItemArray.Add(new XElement("ServerItem",
                            new XElement("name", serverName),
                            new XElement("description", serverDesc),
                            new XElement("connect_string", connectionString),
                            new XElement("enable_login", "true"),
                            new XElement("custom_credentials", "true"),
                            new XElement("default_rodat", "false"),
                            new XElement("default_username", "username"),
                            new XElement("default_password", "password"),
                            new XElement("allow_dual_log", "true"))
                    );
        }
    }
}
