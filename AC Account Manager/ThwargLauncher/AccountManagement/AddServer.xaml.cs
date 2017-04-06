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
        string rodatSetting;
        public AddServer()
        {
            InitializeComponent();
            AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) { return; }

            if (rdACEServer.IsChecked.HasValue && rdACEServer.IsChecked.Value)
            {
                XDocument doc = XDocument.Load("ACEServerList.xml");
                AddNewServerToXmlDoc(doc);
                doc.Save("ACEServerList.xml");
            }
            else if(rdPhatACServer.IsChecked.HasValue && rdPhatACServer.IsChecked.Value)
            {
                XDocument doc = XDocument.Load("PhatACServerList.xml");
                AddNewServerToXmlDoc(doc);
                doc.Save("PhatACServerList.xml");
            }
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtServerName.Text))
            {
                MessageBox.Show("Server Name required");
                txtServerName.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtServerIP.Text))
            {
                MessageBox.Show("Server Address required");
                txtServerIP.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtServerPort.Text))
            {
                MessageBox.Show("Server Port required");
                txtServerPort.Focus();
                return false;
            }
            int portnum = 0;
            if (!int.TryParse(txtServerPort.Text, out portnum))
            {
                MessageBox.Show("Server Port must be numeric");
                txtServerPort.Focus();
                return false;
            }
            if (cmbDefaultRodat.SelectedValue == null)
            {
                MessageBox.Show("Rodat selection required");
                cmbDefaultRodat.Focus();
                return false;
            }
            return true;
        }

        private void AddNewServerToXmlDoc(XDocument doc)
        {
            serverName = txtServerName.Text;
            serverDesc = txtServeDesc.Text;
            serverIP = txtServerIP.Text;
            serverPort = txtServerPort.Text;
            connectionString = serverIP + ":" + serverPort;
            rodatSetting = cmbDefaultRodat.SelectedValue.ToString();

            XElement serverItemArray = doc.Element("ArrayOfServerItem");
            serverItemArray.Add(new XElement("ServerItem",
                            new XElement("name", serverName),
                            new XElement("description", serverDesc),
                            new XElement("connect_string", connectionString),
                            new XElement("enable_login", "true"),
                            new XElement("custom_credentials", "true"),
                            new XElement("default_rodat", rodatSetting),
                            new XElement("default_username", "username"),
                            new XElement("default_password", "password"),
                            new XElement("allow_dual_log", "true"))
                    );
        }
    }
}
