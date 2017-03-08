using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
        }
    }
}
