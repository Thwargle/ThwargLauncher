using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using MagFilter;
using WindowPlacementUtil;

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string arg1;
        public string arg2;
        public string arg3;
        //private Dictionary<string, List<AccountCharacter>> _allAccountCharacters;
        
        private List<string> _Images = new List<string>();
        private System.Random _rand = new Random();
        private BackgroundWorker _worker = new BackgroundWorker();
        private string _launcherLocation;

        public static string UsersFilePath = System.IO.Path.Combine(Configuration.AppFolder, "UserNames.txt");
        private MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            CreateFolderForCurrentUser();

            LoadListBox();
            LoadImages();
            ChangeBackgroundImageRandomly();

            //LoadAllAccountCharacters();
            WireUpBackgroundWorker();

            if (Properties.Settings.Default.ACLocation != "")
            {
                txtLauncherLocation.Text = Properties.Settings.Default.ACLocation;
            }

            lstUsername.SelectedIndex = Properties.Settings.Default.SelectedUser;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            LoadWindowSettings();
        }
        private void LoadWindowSettings()
        {
            this.SetPlacement(Properties.Settings.Default.MainWindowPlacement);
        }
        private void SaveWindowSettings()
        {
            Properties.Settings.Default.MainWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }
        /*
        private void LoadAllAccountCharacters()
        {
            if (_allAccountCharacters != null) { throw new Exception("allAccountCharacters already populated"); }
            _allAccountCharacters = new Dictionary<string, List<AccountCharacter>>();
            var charMgr = MagFilter.CharacterManager.ReadCharacters();
            foreach (string key in charMgr.GetKeys())
            {
                var guys = new List<AccountCharacter>();
                foreach (var dude in charMgr.GetCharacters(key))
                {
                    AccountCharacter guy = new AccountCharacter();
                    guy.Id = dude.Id;
                    guy.Name = dude.Name;
                    guys.Add(guy);
                }
                _allAccountCharacters[key] = guys;
            }
        }
         * */

        private void LoadImages()
        {
            _Images.Clear();
            _Images.Add("acwallpaperwideaerbax.jpg");
            _Images.Add("acwallpaperwideaerfalle.jpg");
            _Images.Add("acwallpaperwideGroup.jpg");
        }

        private string PickRandomImage()
        {
            return _Images[_rand.Next(_Images.Count)];
        }

        private void ChangeBackgroundImageRandomly()
        {
            LoadImages();
            string imageName = PickRandomImage();
            //BigGrid.Background = MyTextBlock.Background;
            ImageBrush brush = new ImageBrush(
                new BitmapImage(
                    new Uri(imageName, UriKind.Relative)
                    ));
            ContentGrid.Background = brush;
        }

        private void CreateFolderForCurrentUser()
        {
            string specificFolder = Configuration.AppFolder;

            // Check if folder exists and if not, create it
            if (!Directory.Exists(specificFolder))
                Directory.CreateDirectory(specificFolder);

            if (!File.Exists(UsersFilePath))
            {
                File.Create(UsersFilePath);
            }
        }

        public void LoadListBox()
        {
            LoadUserAccounts();
        }
        private void LoadUserAccounts()
        {
            var characterMgr = MagFilter.CharacterManager.ReadCharacters();
            _viewModel.Reset();
            using (var reader = new StreamReader(UsersFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) { continue; }
                        string[] arr = line.Split(',');
                        string accountName = arr[0];
                        string password = arr[1];

                        var user = new UserAccount(accountName, characterMgr)
                        {
                            Password = password
                        };
                        _viewModel.KnownUserAccounts.Add(user);
                    }
                }
            }
            this.lstUsername.ItemsSource = null;
            this.lstUsername.ItemsSource = _viewModel.KnownUserAccounts;
        }
        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            List<string> servers = new List<string>();
            servers.Add("TODO");

            if (servers.Count == 0)
            {
                MessageBox.Show("No server selected. Please select a server", "No server selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (lstUsername.SelectedItems.Count == 0)
            {
                MessageBox.Show("No account selected. Please select an account", "No account selected.", MessageBoxButton.OK, MessageBoxImage.Error);
                lstUsername.Focus();
                return;
            }
            LaunchAllClientsOnAllServersOnThread(servers);
        }

        private void WireUpBackgroundWorker()
        {
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += _worker_DoWork;
            _worker.ProgressChanged += _worker_ProgressChanged;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    lblWorkerProgress.Content = "Canceled";
                }
                else if (e.Error != null)
                {
                    lblWorkerProgress.Content = string.Format("Error: {0}", e.Error.Message);
                }
                else
                {
                    lblWorkerProgress.Content = "Done";
                }
            }
            finally
            {
                EnableInterface(true);
            }
        }
        private class WorkerArgs
        {
            public List<string> Servers;
            public List<UserAccount> SelectedAccounts;
        }
        private void LaunchAllClientsOnAllServersOnThread(List<string> servers)
        {
            if (_worker.IsBusy)
            {
                MessageBox.Show("Worker is busy"); // TODO - better message?
            }
            else
            {
                EnableInterface(false);
                // Get data from UI objects before we switch to background thread
                var selectedAccounts = new List<UserAccount>();
                foreach (UserAccount acct in _viewModel.KnownUserAccounts)
                {
                    if (acct.AccountLaunchable)
                    {
                        selectedAccounts.Add(acct);
                    }
                }
                _launcherLocation = txtLauncherLocation.Text;
                WorkerArgs args = new WorkerArgs()
                    {
                        Servers = servers,
                        SelectedAccounts = selectedAccounts
                    };
                _worker.RunWorkerAsync(args);
            }
        }
        private void EnableInterface(bool enable)
        {
            btnLaunch.IsEnabled = enable;
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblWorkerProgress.Content = string.Format("{0}%", e.ProgressPercentage);
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs args = (e.Argument as WorkerArgs);
//            int serverIndex = 0;
//            int serverTotal = ServerManager.ServerList; // TODO - this logic is not right at all

            foreach (UserAccount account in args.SelectedAccounts)
            {
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                LaunchClientsForAccount(account);
            }
        }

        private void LaunchClientsForAccount(UserAccount account)
        {
            foreach (var server in account.Servers)
            {
                if (!server.ServerSelected) { continue; }
                string desiredCharacter = server.ChosenCharacter;
                bool okgo = LaunchGameClient(server.ServerName, account, desiredCharacter);
                if (!okgo) { break; }
                // TODO - wait for client
                System.Threading.Thread.Sleep(15000);
            }
//            int pct = (int)(100.0 * serverIndex / serverTotal);
//            _worker.ReportProgress(pct);
        }
        private Server FindSpecifiedServer(UserAccount account, string serverName)
        {
            foreach (Server server in account.Servers)
            {
                if (server.ServerName == serverName)
                {
                    return server;
                }
            }
            return null;
        }
        private bool LaunchGameClient(string serverName, UserAccount account, string desiredCharacter)
        {
            //-username "MyUsername" -password "MyPassword" -w "ServerName" -2 -3
            if (account == null) { ShowMessage("Denied"); return false; }
            arg1 = account.Name;
            arg2 = account.Password;
            arg3 = serverName;

            string genArgs = "-username " + arg1 + " -password " + arg2 + " -w " + arg3 + " -2 -3";
            string pathToFile = _launcherLocation;
            Process runProg = new Process();
            if (arg2 == "")
            {
                genArgs = "-username " + arg1 + " -w " + arg3 + " -3 ";
            }
            try
            {
                runProg.StartInfo.FileName = pathToFile;
                runProg.StartInfo.Arguments = genArgs;
                runProg.StartInfo.CreateNoWindow = true;

                RecordLaunchInfo(serverName, account.Name, desiredCharacter);

                // This is analogous to Process.Start or CreateProcess
                runProg.Start();
            }
            catch (Exception ex)
            {
                ShowMessage("Could not start program. Please Check your path. ", "Launcher not found.", MessageBoxButton.OK, MessageBoxImage.Error);
                if (arg2 != "")
                {
                    return false;
                }
            }
            if (arg2 == "")
            {
                ShowMessage("Multiple Logins Stopped. You don't have a password set, and multiple logins cannot continue.", "Multiple Logins Stopped.", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private void RecordLaunchInfo(string serverName, string accountName, string desiredCharacter)
        {
            var ctl = new LaunchControl();
            ctl.RecordLaunchInfo(serverName: serverName, accountName: accountName, characterName: desiredCharacter);
        }

        private void ShowMessage(string msg)
        {
            ShowMessage(msg, "Caption", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ShowMessage(string msg, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(msg, caption, button, image));
        }

        private void OpenLauncherLocation()
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = "C:\\Turbine\\Asheron's Call";

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".exe";
            dlg.Filter = "Executables (exe)|*.exe|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                txtLauncherLocation.Text = filename;
            }
        }

        private void txtLauncherLocation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenLauncherLocation();
        }

        private void btnLauncherLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenLauncherLocation();
        }

        private void AC_Account_Manager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();

            Properties.Settings.Default.SelectedUser = lstUsername.SelectedIndex;
            Properties.Settings.Default.ACLocation = txtLauncherLocation.Text;
            
            Properties.Settings.Default.Save();
        }

        private void btnAddUsers_Click(object sender, RoutedEventArgs e)
        {
            MainWindowDisable();
            AddUsers add = new AddUsers();
            add.ShowDialog();
            LoadUserAccounts();
            MainWindowEnable();
        }
        private void MainWindowDisable()
        {
            this.ContentGrid.Background = new SolidColorBrush(Colors.Pink);
        }
        private void MainWindowEnable()
        {
            ChangeBackgroundImageRandomly();
        }
        private void btnOpenUsers_Click(object sender, RoutedEventArgs e)
        {
            MainWindowDisable();

            var startInfo = new ProcessStartInfo("notepad", UsersFilePath);
            var notepadProcess = new Process() {StartInfo = startInfo};
            if (notepadProcess.Start())
            {
                notepadProcess.WaitForExit();
            }
            LoadUserAccounts();
            MainWindowEnable();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            bool isWindowOpen = false;

            foreach (Window w in Application.Current.Windows)
            {
                {
                    if (w is Help)
                        isWindowOpen = true;
                        w.Activate();
                }
            }

            if (!isWindowOpen)
            {
                Help newwindow = new Help();
                newwindow.Show();
            }
        }
    }
}
