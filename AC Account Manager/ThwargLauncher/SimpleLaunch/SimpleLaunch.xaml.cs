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
using System.Windows.Shapes;
using WindowPlacementUtil;

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for SimpleLaunch.xaml
    /// </summary>
    public partial class SimpleLaunch : Window
    {
        public event LaunchGameDelegateMethod LaunchingEvent;
        private SimpleLaunchWindowViewModel _viewModel;
        //private List<Server.ServerItem> sl = new List<Server.ServerItem>();
        public SimpleLaunch(SimpleLaunchWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            LoadWindowSettings();
        }
        private void LoadWindowSettings()
        {
            var x = this.Owner;
            this.chkUserDecal.IsChecked = Properties.Settings.Default.InjectDecal;
        }
        private void SaveWindowSettings()
        {
            Properties.Settings.Default.InjectDecal = this.chkUserDecal.IsChecked.Value;
            Properties.Settings.Default.Save();
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

            string path = Properties.Settings.Default.ACLocation; // "c:\\Turbine\\Asheron's Call\\acclient.exe";
           LaunchSimpleGame(path, _viewModel.SelectedServer, _viewModel.AccountName, _viewModel.Password);
        }

        private void LaunchSimpleGame(string path, Server.ServerItem server, string account, string pwd)
        {
            SaveWindowSettings();
            var launchItem = new LaunchItem();
            launchItem.CustomLaunchPath = path;
            launchItem.ServerName = server.ServerName;
            launchItem.AccountName = account;
            launchItem.Password = pwd;
            launchItem.ipAddress = server.ServerIP;
            launchItem.EMU = server.EMU;
            launchItem.CharacterSelected = null; // no character choices for SimpleLaunch, b/c that requires MagFilter
            launchItem.RodatSetting = server.RodatSetting;
            launchItem.IsSimpleLaunch = true;

            LaunchingEvent(launchItem);

            ////var launcher = new GameLauncher();
            ////GameLaunchResult glr = launcher.LaunchGameClient(path, server.ServerName, account, pwd, server.ServerIP, server.EMU, null, server.RodatSetting);
            ////return glr;
        }

        private void ThwargLauncherSimpleLaunchWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
            foreach (Window w in App.Current.Windows)
            {
                if(w != this)
                    w.Show();
            }
        }
    }
}
