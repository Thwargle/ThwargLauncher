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
    public partial class SimpleLaunchWindow : Window
    {
        private SimpleLaunchWindowViewModel _viewModel;
        //private List<ServerInfo> sl = new List<ServerInfo>();
        public SimpleLaunchWindow(SimpleLaunchWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
            _viewModel.CloseAction = new Action(() => this.Close());
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            LaunchGame();
        }

        public void LaunchGameCommand(object sender, ExecutedRoutedEventArgs e)
        {
            LaunchGame();
        }
        public void LaunchGame()
        {
            Properties.Settings.Default.ACLocation = txtLauncherLocation.Text;
            Properties.Settings.Default.Save();
            if (_viewModel.SelectedServer == null)
            {
                MessageBox.Show("Please select a server.", "No server selected.");
                cmbServerList.Focus();
                return;
            }
            if (!IsValidUserName(_viewModel.AccountName))
            {
                txtUserName.Focus();
                return;
            }
            if (!IsValidPassword(_viewModel.AccountName, _viewModel.Password))
            {
                txtUserPassword.Focus();
                return;
            }
            _viewModel.PerformSimpleLaunch();
        }

        private bool IsValidUserName(string text)
        {
            if (text.Contains("!") ||
                text.Contains("@") ||
                text.Contains("#") ||
                text.Contains("$") ||
                text.Contains("%") ||
                text.Contains("^") ||
                text.Contains("&") ||
                text.Contains("*") ||
                text.Contains("(") ||
                text.Contains(")") ||
                text.Contains("=") ||
                text.Contains(".") ||
                text.Contains(",") ||
                text.Contains("<") ||
                text.Contains(">") ||
                text.Contains("?") ||
                text.Contains(";") ||
                text.Contains(":")
                )
            {
                var msg = string.Format("Name '{0}' contains an invalid character. Please do not use !@#$%^&*()=.,<>?;:", text);
                MessageBox.Show(msg, "Invalid name");
                return false;
            }
            return true;
        }

        private bool IsValidPassword(string name, string password)
        {
            if (password.Contains(" "))
            {
                var msg = string.Format("Password for account '{0}' may not contain a space.", name);
                MessageBox.Show(msg, "Invalid password.");
                return false;
            }
            return true;
        }

        private void ThwargLauncherSimpleLaunchWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.SaveToSettings();
        }

        private void txtLauncherLocation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _viewModel.ConfigureFileLocationCommand.Execute(null);
        }
    }
}
