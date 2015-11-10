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
using WindowPlacementUtil;

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private Dictionary<string, List<AccountCharacter>> _allAccountCharacters;
        
        private List<string> _Images = new List<string>();
        private System.Random _rand = new Random();
        private BackgroundWorker _worker = new BackgroundWorker();
        private string _launcherLocation;

        public static string UsersFilePath = System.IO.Path.Combine(Configuration.AppFolder, "UserNames.txt");
        private MainWindowViewModel _viewModel = new MainWindowViewModel();
        private string _currentProfileName = "Profile1";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            CreateFolderForCurrentUser();

            LoadUserAccounts(initialLoad: true);
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
                    new Uri("Images\\" + imageName, UriKind.Relative)
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
                var fileStream = File.Create(UsersFilePath);
                fileStream.Close();
            }
        }

        private void LoadUserAccounts(bool initialLoad = false)
        {
            if (!initialLoad) // we do not save the first time, because have never yet loaded
            {
                SaveCurrentProfile();
            }
            ReloadKnownAccountsAndCharacters();
            LoadCurrentProfile();
        }
        private void SaveCurrentProfile()
        {
            List<CharacterSetting> settings = GetCurrentProfileSettingsFromModel();
            ProfileManager mgr = new ProfileManager();
            mgr.Save(settings, _currentProfileName);
        }
        private List<CharacterSetting> GetCurrentProfileSettingsFromModel()
        {
            return (from account in _viewModel.KnownUserAccounts
                    from server in account.Servers
                    select new CharacterSetting()
                        {
                            AccountName = account.Name,
                            ServerName = server.ServerName,
                            ChosenCharacter = server.ChosenCharacter
                        }).ToList();
        }
        private void LoadCurrentProfile()
        {
            ProfileManager mgr = new ProfileManager();
            var profSettings = mgr.Load(_currentProfileName);
            if (profSettings != null)
            {
                ApplyProfileSettingsToModel(profSettings);
            }
        }
        private void ApplyProfileSettingsToModel(List<CharacterSetting> profSettings)
        {
            foreach (var account in _viewModel.KnownUserAccounts)
            {
                foreach (var server in account.Servers)
                {
                    var setting = profSettings.Find(x => x.AccountName == account.Name
                        && x.ServerName == server.ServerName);
                    if (setting != null)
                    {
                        server.ChosenCharacter = setting.ChosenCharacter;
                    }
                }
            }
        }
        private void ReloadKnownAccountsAndCharacters()
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
            _launcherLocation = txtLauncherLocation.Text;
            if (string.IsNullOrEmpty(_launcherLocation))
            {
                ShowErrorMessage("Game launcher location required");
                return;
            }
            if (!File.Exists(_launcherLocation))
            {
                ShowErrorMessage(string.Format("Game launcher missing: '{0}'", _launcherLocation));
                return;
            }
            if (!CheckAccountsAndPasswords())
            {
                return;
            }
            LaunchAllClientsOnAllServersOnThread();
        }
        private bool CheckAccountsAndPasswords()
        {
            int count = 0;
            foreach (var account in _viewModel.KnownUserAccounts)
            {
                if (account.AccountLaunchable)
                {
                    foreach (var server in account.Servers)
                    {
                        if (server.ServerSelected)
                        {
                            ++count;
                            if (string.IsNullOrWhiteSpace(account.Name))
                            {
                                ShowErrorMessage("Blank account not allowed");
                                return false;
                            }
                            if (string.IsNullOrWhiteSpace(account.Password))
                            {
                                ShowErrorMessage("Black password not allowed");
                                return false;
                            }
                        }
                    }
                }
            }
            if (count == 0)
            {
                ShowErrorMessage("No accounts and servers selected");
                return false;
            }
            return true;
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
                    lblWorkerProgress.Content = "Worker Progress: Canceled";
                }
                else if (e.Error != null)
                {
                    lblWorkerProgress.Content = string.Format("Error: {0}", e.Error.Message);
                }
                else
                {
                    lblWorkerProgress.Content = "Worker Progress: Done";
                }
            }
            finally
            {
                EnableInterface(true);
            }
        }
        private class WorkerArgs
        {
            public LaunchSorter.LaunchList LaunchList;
        }
        private void LaunchAllClientsOnAllServersOnThread()
        {
            if (_worker.IsBusy)
            {
                lblWorkerProgress.Content = "Worker Progress: Busy";
            }
            else
            {
                EnableInterface(false);
                var launchMgr = new LaunchSorter();
                LaunchSorter.LaunchList launchList = launchMgr.GetLaunchList(_viewModel.KnownUserAccounts);
                WorkerArgs args = new WorkerArgs()
                    {
                        LaunchList = launchList
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
            lblWorkerProgress.Content = string.Format(
                "{0}% - {1}",
                e.ProgressPercentage,
                e.UserState.ToString()
                );
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs args = (e.Argument as WorkerArgs);
            if (args == null) { return; }
            int serverIndex = 0;
            int serverTotal = args.LaunchList.GetLaunchItemCount();

            foreach (var launchItem in args.LaunchList.GetLaunchList())
            {
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                {
                    int pct = (int)(100.0 * serverIndex / serverTotal);
                    string context = string.Format(
                        "Launching account {0} on server {1}",
                        launchItem.AccountName, launchItem.ServerName);
                    _worker.ReportProgress(pct, context);
                }

                var launcher = new GameLauncher();
                try
                {
                    bool okgo = launcher.LaunchGameClient(
                        _launcherLocation,
                        launchItem.ServerName,
                        accountName: launchItem.AccountName,
                        password: launchItem.Password,
                        desiredCharacter: launchItem.CharacterSelected
                        );
                    if (!okgo)
                    {
                        break;
                    }
                }
                catch (Exception exc)
                {
                    ShowErrorMessage("Exception launching game launcher: " + exc.Message);
                    break;
                }
                // TODO - wait for client

                ++serverIndex;
                {
                    int pct = (int)(100.0 * serverIndex / serverTotal);
                    string context = string.Format(
                        "Launched account {0} on server {1}",
                        launchItem.AccountName, launchItem.ServerName);
                    _worker.ReportProgress(pct, context);
                }
                
                System.Threading.Thread.Sleep(15000);
               
            }
        }

        private void ShowErrorMessage(string msg)
        {
            ShowMessage(msg, "Caption", MessageBoxButton.OK, MessageBoxImage.Information);
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
            SaveCurrentProfile();

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
