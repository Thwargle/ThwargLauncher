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
                string filepath = ServerManager.GetAceServerFilepath();
                XDocument doc = XDocument.Load(filepath);
                AddNewServerToXmlDoc(doc);
                doc.Save(filepath);
            }
            else if(rdPhatACServer.IsChecked.HasValue && rdPhatACServer.IsChecked.Value)
            {
                string filepath = ServerManager.GetPhatServerFilepath();
                XDocument doc = XDocument.Load(filepath);
                AddNewServerToXmlDoc(doc);
                doc.Save(filepath);
            }
            this.DialogResult = true;
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
            var server = new GameManagement.ServerPersister.EditedServerInfo()
            {
                ServerName =  txtServerName.Text,
                ServerDesc = txtServeDesc.Text,
                ConnectionString = txtServerIP.Text + ":" + txtServerPort.Text,
                RodatSetting = cmbDefaultRodat.SelectedValue.ToString()
            };
            // Used by both ACE & Phat
            GameManagement.ServerPersister.AddNewServerToXmlDoc(server, doc);
        }
    }
}
