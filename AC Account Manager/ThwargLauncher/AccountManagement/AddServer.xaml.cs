using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using WindowPlacementUtil;
using ThwargLauncher.UtilityCode;

namespace ThwargLauncher.AccountManagement
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        public AddServer()
        {
            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            LoadWindowSettings();
        }
        private void LoadWindowSettings()
        {
            this.SetPlacement(Properties.Settings.Default.AddServerWindowPlacement);
        }
        private void SaveWindowSettings()
        {
            Properties.Settings.Default.AddServerWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (rdACEServer.IsChecked.HasValue && rdACEServer.IsChecked.Value)
            {
                const string aceServerFilepath = "ACEServerList.xml";
                GameManagement.AceServerLister lister = new GameManagement.AceServerLister();
                var servers = lister.loadACEServers();
                var distinctServers = servers.DistinctBy(s => s.ServerName.ToUpper());
                var serverNames = distinctServers.ToDictionary(s => s.ServerName.ToUpper());
                if (serverNames.ContainsKey(txtServerName.Text.ToUpper()))
                {
                    MessageBox.Show("There is already an ACE server with this name");
                    txtServerName.Focus();
                    return;
                }
                XDocument doc = XDocument.Load(aceServerFilepath);
                createElement(doc);
                doc.Save(aceServerFilepath);
            }
            else if(rdPhatACServer.IsChecked.HasValue && rdPhatACServer.IsChecked.Value)
            {
                const string pathServerFilepath = "PhatACServerList.xml";
                var servers = GameManagement.PhatACServerLister.ReadPhatServerList(pathServerFilepath);
                var distinctServers = servers.DistinctBy(s => s.ServerName.ToUpper());
                var serverNames = distinctServers.ToDictionary(s => s.ServerName.ToUpper());
                if (serverNames.ContainsKey(txtServerName.Text.ToUpper()))
                {
                    MessageBox.Show("There is already a Phat server with this name");
                    txtServerName.Focus();
                    return;
                }
                XDocument doc = XDocument.Load(pathServerFilepath);
                createElement(doc);
                doc.Save(pathServerFilepath);
            }
            this.Close();
        }

        private void createElement(XDocument doc)
        {
            string serverName = txtServerName.Text;
            string serverDesc = txtServeDesc.Text;
            string serverIP = txtServerIP.Text;
            string serverPort = txtServerPort.Text;
            string connectionString = serverIP + ":" + serverPort;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
        }
    }
}
