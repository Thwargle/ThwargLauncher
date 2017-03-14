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

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for SimpleLaunch.xaml
    /// </summary>
    public partial class SimpleLaunch : Window
    {
        private SimpleLaunchWindowViewModel _viewModel;
        public SimpleLaunch(SimpleLaunchWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            LoadServers();
        }

        public void LoadServers()
        {
            foreach (Server.ServerItem si in ServerManager.ServerList)
            {
                var serverComboItem = new KeyValuePair<string, string>(si.ServerName, si.ServerIP);
                cmbServerList.Items.Add(serverComboItem);
            }
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (cmbServerList.SelectedItem == null)
            {
                MessageBox.Show("Please select a server.", "No server selected.");
            }
        }
    }
}
