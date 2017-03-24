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
        //private List<Server.ServerItem> sl = new List<Server.ServerItem>();
        public SimpleLaunch(SimpleLaunchWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedServer == null)
            {
                MessageBox.Show("Please select a server.", "No server selected.");
                cmbServerList.Focus();
                return;
            }
            if (string.IsNullOrEmpty(_viewModel.AccountName))
            {
                MessageBox.Show("Please enter an acount name.", "No account selected.");
                txtUserName.Focus();
                return;
            }

            //TODO: Get actual info to pass into launch command
            string path = "c:\\Turbine\\Asheron's Call\\acclient.exe";
            GameLaunchResult glr = LaunchSimpleGame(path, _viewModel.SelectedServer, _viewModel.AccountName, _viewModel.Password);
        }
        private GameLaunchResult LaunchSimpleGame(string path, Server.ServerItem server, string account, string pwd)
        {
            var launcher = new GameLauncher();
            GameLaunchResult glr = launcher.LaunchGameClient(path, server.ServerName, account, pwd, server.ServerIP, server.EMU, null, server.RodatSetting);
            return glr;
        }
    }
}
