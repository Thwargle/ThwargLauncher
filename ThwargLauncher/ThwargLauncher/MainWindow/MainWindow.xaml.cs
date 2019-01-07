using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ThwargLauncher.AccountManagement;

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private Dictionary<string, List<AccountCharacter>> _allAccountCharacters;

        private List<string> _Images = new List<string>();
        private Random _rand = new Random();
        private BackgroundWorker _worker = new BackgroundWorker();
        private string _clientExeLocation;

        private readonly MainWindowViewModel _viewModel;
        private readonly GameSessionMap _gameSessionMap;
        private readonly GameMonitor _gameMonitor;
        private readonly LaunchWorker _launchWorker;

        readonly SynchronizationContext _uicontext;
        private System.Timers.Timer _timer;

        public static string OldUsersFilePath = Path.Combine(Configuration.AppFolder, "UserNames.txt");

        private System.Collections.Concurrent.ConcurrentQueue<LaunchItem> _launchConcurrentQueue =
            new System.Collections.Concurrent.ConcurrentQueue<LaunchItem>();

        internal MainWindow(MainWindowViewModel mainWindowViewModel, GameSessionMap gameSessionMap, GameMonitor gameMonitor)
        {
            if (mainWindowViewModel == null) { throw new Exception("Null MainWindowViewModel in MainWindow()"); }
            if (gameSessionMap == null) { throw new Exception("Null GameSessionMap in MainWindow()"); }
            if (gameMonitor == null) { throw new Exception("Null GameMonitor in MainWindow()"); }

            _viewModel = mainWindowViewModel;
            _viewModel.CloseAction = new Action(() => this.Close());

            _gameSessionMap = gameSessionMap;
            _gameMonitor = gameMonitor;
            _launchWorker = new LaunchWorker(_worker, _gameSessionMap);
            _launchWorker.ReportAccountStatusEvent += (accountStatus, item) => UpdateAccountStatus(accountStatus, item);
            _launchWorker.ProgressChangedEvent += _worker_ProgressChanged;
            _launchWorker.WorkerCompletedEvent += _worker_RunWorkerCompleted;
            _uicontext = SynchronizationContext.Current;

            _viewModel.OpeningSimpleLauncherEvent += () => this.Hide();
            _viewModel.LaunchingSimpleGameEvent += (li) => this.LaunchSimpleClient(li);

            if (Properties.Settings.Default.AutoRelaunch)
            {
                CheckForProgramUpdate();
            }

            InitializeComponent();
            DataContext = _viewModel;
            mainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;

            EnsureDataFoldersExist();

            PopulateServerList();

            LoadUserAccounts(initialLoad: true);
            LoadImages();
            ChangeBackgroundImageRandomly();

            SubscribeToGameMonitorEvents();
            _timer = new System.Timers.Timer(5000); // every five seconds
            _timer.Elapsed += _timer_Elapsed;
            StartStopTimerIfAutoChecked();


            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        void StartStopTimerIfAutoChecked()
        {
            _timer.Enabled = TryGetAutoRelaunch();
        }
        private bool TryGetAutoRelaunch()
        {
            try
            {
                return Properties.Settings.Default.AutoRelaunch;
            }
            catch
            {
                return false;
            }
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsLaunchDue())
            {
                InvokeLaunchOnAppropriateThread();
            }
        }
        private bool IsLaunchDue()
        {
            return _launchWorker.IsLaunchDue();
        }
        private void SubscribeToGameMonitorEvents()
        {
            _gameMonitor.GameDiedEvent += _gameMonitor_GameDiedEvent;
        }
        private void UnsubscribeToGameMonitorEvents()
        {
            _gameMonitor.GameDiedEvent -= _gameMonitor_GameDiedEvent;
        }
        private void _gameMonitor_GameDiedEvent(object sender, EventArgs e)
        {
            _launchWorker.RequestImmediateLaunch();
        }
        private void InvokeLaunchOnAppropriateThread()
        {
            Application.Current.Dispatcher.Invoke(() =>
                LaunchAllClientsOnAllServersOnThread());
        }

        private void PopulateServerList()
        {
            ServerManager.LoadServerLists();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (TryGetShowHelpAtStart())
            {
                DisplayHelpWindow();
            }
        }
        private bool TryGetShowHelpAtStart()
        {
            try
            {
                return Properties.Settings.Default.ShowHelpAtStart;
            }
            catch
            {
                return true;
            }
        }
        private void CheckForProgramUpdate()
        {
            // run the updater application if it is where we expect it to be (installation root), and wait for it to finish before loading
            if (System.IO.File.Exists("updater.exe"))
            {
                // if we would like to get the exit code of the updater app and log it later, we have it.
                int exitCode;
                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = "/silent";
                info.FileName = "updater.exe";
                using (Process proc = Process.Start(info))
                {
                    proc.WaitForExit();
                    exitCode = proc.ExitCode;
                }
            }
        }
        void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO - this is a workaround
            if (e.PropertyName == "ShownUserAccounts")
            {
                this.lstUsername.ItemsSource = null;
                this.lstUsername.ItemsSource = _viewModel.ShownUserAccounts;
            }
            if (e.PropertyName == "AutoRelaunch")
            {
                StartStopTimerIfAutoChecked();
            }
        }
        private void LoadImages()
        {
            _Images.Clear();
            _Images.Add("acwallpaperwideaerbax.jpg");
            _Images.Add("acwallpaperwideaerfalle.jpg");
            _Images.Add("acwallpaperwideGroup.jpg");
            _Images.Add("acwallpaperwide10yrs.jpg");
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

        private void EnsureDataFoldersExist()
        {
            // Ensure program data folder exists
            string specificFolder = Configuration.AppFolder;
            if (!Directory.Exists(specificFolder))
            {
                Directory.CreateDirectory(specificFolder);
            }
            // Ensure all data folders shared with filter exist
            ThwargFilter.FileLocations.EnsureAllDataFoldersExist();

            // Ensure profiles folder exists
            var mgr = new ProfileManager();
            mgr.EnsureProfileFolderExists();
        }

        /// <summary>
        /// Forward call to our LoadUserAccounts method on ui thread
        /// </summary>
        private void CallUiLoadUserAccounts()
        {
            object state = null;
            _uicontext.Post(new SendOrPostCallback(
                (obj) => LoadUserAccounts()), state);
        }
        private void CallUiNotifyAvailableCharactersChanged()
        {
            object state = null;
            _uicontext.Post(new SendOrPostCallback(
                (obj) => NotifyAvailableCharactersChanged()), state);
        }
        private void NotifyAvailableCharactersChanged()
        {
            _viewModel.NotifyAvailableCharactersChanged();
        }
        private void LoadUserAccounts(bool initialLoad = false)
        {
            _viewModel.CreateProfileIfDoesNotExist();
            if (!initialLoad) // we do not save the first time, because have never yet loaded
            {
                SaveCurrentProfile();
            }
            ReloadKnownAccountsAndCharacters();
            try
            {
                _viewModel.LoadMostRecentProfile();
            }
            catch
            {
                ShowErrorMessage("Error loading profile");
            }
        }
        private void SaveCurrentProfile()
        {
            _viewModel.SaveCurrentProfile();
        }
        private void ReloadKnownAccountsAndCharacters()
        {
            _viewModel.ReloadAccounts(OldUsersFilePath);
            lstUsername.ItemsSource = null;
            lstUsername.ItemsSource = _viewModel.KnownUserAccounts;
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
            _viewModel.ClientFileLocation = txtLauncherLocation.Text;
            _clientExeLocation = _viewModel.ClientFileLocation;
            if (string.IsNullOrEmpty(_clientExeLocation))
            {
                ShowErrorMessage("Game launcher location required");
                return;
            }
            if (!File.Exists(_clientExeLocation))
            {
                ShowErrorMessage(string.Format("Game launcher missing: '{0}'", _clientExeLocation));
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
            var accounts = new Dictionary<string, int>();
            int numberBlankPasswords = 0;
            foreach (var account in _viewModel.KnownUserAccounts)
            {
                if (account.AccountLaunchable)
                {
                    accounts[account.AccountName] = 1;
                    foreach (var server in account.Servers)
                    {
                        if (server.ServerSelected)
                        {
                            ++count;
                            if (string.IsNullOrWhiteSpace(account.AccountName))
                            {
                                // We try not to let this happen, so it should be very rare
                                ShowErrorMessage("Blank account not allowed");
                                return false;
                            }
                            if (string.IsNullOrWhiteSpace(account.Password))
                            {
                                ++numberBlankPasswords;
                            }
                        }
                    }
                }
            }
            if (accounts.Keys.Count > 1 && numberBlankPasswords > 0)
            {
                ShowErrorMessage("Blank passwords are not allowed when launching multiple accounts");
                return false;
            }
            if (count == 0)
            {
                ShowErrorMessage("No accounts and servers selected");
                return false;
            }
            return true;
        }


        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Just wait - the game monitor checks the character periodically
            // and the account manager is subscribed on it for changes
            // _viewModel.ReloadCharacters();
            // _viewModel.NotifyAvailableCharactersChanged();
            // It would be nice to call NotifyAvailableCharactersChanged()
            // But the properties haven't actually changed yet
            // The characters file needs to be reread from disk & the properties updated from it
            // LoadUserAccounts();

            try
            {
                if (e.Cancelled)
                {
                    lblWorkerProgress.Content = "User Cancelled";
                    ClearLaunchQueue();
                }
                else if (e.Error != null)
                {
                    lblWorkerProgress.Content = string.Format("Error: {0}", e.Error.Message);
                }
                else
                {
                    lblWorkerProgress.Content = "Launcher Complete";
                }
                _gameSessionMap.EndAllLaunchingSessions();
            }
            finally
            {
                EnableInterface(true);
                btnCancel.IsEnabled = false;
            }
        }
        private void ClearLaunchQueue()
        {
            {
                LaunchItem item;
                while (_launchConcurrentQueue.TryDequeue(out item))
                {
                }
            }
        }
        private void LaunchSimpleClient(LaunchItem launchItem)
        {
            if (_worker.IsBusy)
            {
                lblWorkerProgress.Content = "Launcher In Use";
            }
            else
            {
                Properties.Settings.Default.Save();
                _launchConcurrentQueue.Enqueue(launchItem);
                EnableInterface(false);
                btnCancel.IsEnabled = true;
                _launchWorker.LaunchQueue(_launchConcurrentQueue, _clientExeLocation);
            }
        }
        private void LaunchAllClientsOnAllServersOnThread()
        {
            _viewModel.ClientFileLocation = txtLauncherLocation.Text;
            _clientExeLocation = _viewModel.ClientFileLocation;
            if (_worker.IsBusy)
            {
                lblWorkerProgress.Content = "Launcher In Use";
            }
            else
            {
                Properties.Settings.Default.Save();
                EnableInterface(false);
                btnCancel.IsEnabled = true;
                UpdateConcurrentQueue();
                _viewModel.RecordProfileLaunch();
                _launchWorker.LaunchQueue(_launchConcurrentQueue, _clientExeLocation);
            }
        }
        private void UpdateConcurrentQueue()
        {
            var launchSorter = new LaunchSorter();
            var launchList = GetLaunchListFromAccountList(_viewModel.KnownUserAccounts.Select(x => x.Account));
            launchList = launchSorter.SortLaunchList(launchList);
            foreach (var item in launchList.GetLaunchList())
            {
                _launchConcurrentQueue.Enqueue(item);
            }
            _gameMonitor.QueueReread();
        }
        private void EnableInterface(bool enable)
        {
            btnLaunch.IsEnabled = enable;
        }
        /// <summary>
        /// Get all server/accounts that are checked to be launched
        /// AND that are not already running
        /// </summary>
        /// <param name="accountList"></param>
        /// <returns></returns>
        private LaunchSorter.LaunchList GetLaunchListFromAccountList(IEnumerable<UserAccount> accountList)
        {
            var launchList = new LaunchSorter.LaunchList();
            foreach (var account in accountList)
            {
                if (account.AccountLaunchable)
                {
                    foreach (var server in account.Servers)
                    {
                        if (server.ServerSelected)
                        {
                            var state = _gameSessionMap.GetGameSessionStateByServerAccount(serverName: server.ServerName, accountName: account.Name);
                            if (state != ServerAccountStatusEnum.None)
                            {
                                continue;
                            }
                            var launchItem = new LaunchItem()
                            {
                                AccountName = account.Name,
                                Priority = account.Priority,
                                Password = account.Password,
                                ServerName = server.ServerName,
                                IpAndPort = server.ServerIpAndPort,
                                GameApiUrl = server.GameApiUrl,
                                LoginServerUrl = server.LoginServerUrl,
                                DiscordUrl = server.DiscordUrl,
                                EMU = server.EMU,
                                CharacterSelected = server.ChosenCharacter,
                                CustomLaunchPath = account.CustomLaunchPath,
                                CustomPreferencePath = account.CustomPreferencePath,
                                RodatSetting = server.RodatSetting,
                                SecureSetting = server.SecureSetting
                            };
                            launchList.Add(launchItem);
                        }
                    }
                }
            }
            return launchList;
        }

        private class ProgressInfo { public int Index; public int Total; public string Message; }
        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressInfo info = (e.UserState as ProgressInfo);
            lblWorkerProgress.Content = string.Format(
                "{0}% - {1}/{2} - {3}",
                e.ProgressPercentage,
                info.Index, info.Total,
                info.Message
                );
        }
        private void UpdateAccountStatus(ServerAccountStatusEnum status, LaunchItem launchItem)
        {
            _viewModel.UpdateAccountStatus(launchItem.ServerName, launchItem.AccountName, status);
        }
        public static void ShowErrorMessage(string msg)
        {
            ShowMessage(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public static void ShowMessage(string msg)
        {
            ShowMessage(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void ShowMessage(string msg, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(msg, caption, button, image));
        }
        private void OpenLauncherLocation()
        {
            _viewModel.ChooseLauncherLocation();
        }
        private void txtLauncherLocation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenLauncherLocation();
        }

        private void btnLauncherLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenLauncherLocation();
        }

        private void ThwargLauncherMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Should stop background worker, but that is not straightforward
            UnsubscribeToGameMonitorEvents();
            _timer.Enabled = false;

            _viewModel.WindowClosing();

            Properties.Settings.Default.SelectedUser = lstUsername.SelectedIndex;

            Properties.Settings.Default.Save();
        }

        private void btnAddUsers_Click(object sender, RoutedEventArgs e)
        {
            MainWindowDisable();
            var dlg = new AddUsers(_viewModel.KnownUserAccounts.Select(x => x.Account));
            dlg.ShowDialog();
            LoadUserAccounts();
            MainWindowEnable();
        }
        private void btnChooseProfile_Click(object sender, RoutedEventArgs e)
        {
            MainWindowDisable();
            var dlg = new ChooseProfile(_viewModel);
            var result = dlg.ShowDialog();
            MainWindowEnable();
            if (result.HasValue && result.Value && !string.IsNullOrEmpty(dlg.ProfileNameChosen))
            {
                string desiredProfileName = dlg.ProfileNameChosen;
                _viewModel.GotoSpecificProfile(desiredProfileName);
            }
            else
            {
                _viewModel.LoadMostRecentProfile();
            }
        }
        private void MainWindowDisable()
        {
            rctBlack.Fill = new SolidColorBrush(Colors.Gainsboro);
        }
        private void MainWindowEnable()
        {
            rctBlack.Fill = new SolidColorBrush(Colors.Black);
        }
        private void btnLogWindow_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DisplayLogWindow();
        }
        private void btnOpenUsers_Click(object sender, RoutedEventArgs e)
        {
            MainWindowDisable();

            var startInfo = new ProcessStartInfo("notepad", AccountParser.AccountFilePath);
            var notepadProcess = new Process() { StartInfo = startInfo };
            if (notepadProcess.Start())
            {
                notepadProcess.WaitForExit();
            }
            LoadUserAccounts();
            MainWindowEnable();
        }
        private void btnEditUsers_Click(object sender, RoutedEventArgs e)
        {
            AccountEditorViewModel acevm = new AccountEditorViewModel(this._viewModel.KnownUserAccounts.Select(x => x.Account));

            AccountEditor dlg = new AccountEditor();
            dlg.DataContext = acevm;
            dlg.ShowDialog();
            LoadUserAccounts();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            DisplayHelpWindow();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            btnCancel.IsEnabled = false;
            _worker.CancelAsync();
        }
        private void DisplayHelpWindow()
        {
            _viewModel.DisplayHelpWindow();
        }

        private void txtProfileName_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnChooseProfile_Click(sender, e);
        }

        private void btnEditServers_Click(object sender, RoutedEventArgs e)
        {
            MainWindowDisable();

            EditServersViewModel vm = new EditServersViewModel();
            EditServersWindow win = new EditServersWindow(vm);
            win.ShowDialog();

            // Save any changes the user made to disk
            ServerManager.SaveServerListToDisk();

            if (vm.AddServerRequested)
            {
                var dlg = new AddServer();
                var result = dlg.ShowDialog();
                if (IsTrue(result))
                {
                    PopulateServerList();
                    LoadUserAccounts(initialLoad: false);
                }
            }
            MainWindowEnable();
        }
        private void btnSimpleLaunch_Click(object sender, RoutedEventArgs e)
        {
            string exepath = Properties.Settings.Default.ACLocation;
            if (!System.IO.File.Exists(exepath))
            {
                MessageBox.Show("Client exe not found: " + exepath, "Launcher configuration error");
                //return;
            }
            _viewModel.CallSimpleLauncher();
            this.Hide();
        }
        private static bool IsTrue(bool? bval, bool defval = false)
        {
            return (bval.HasValue ? bval.Value : defval);
        }

        private void CheckRelaunch()
        {

        }
        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.OriginalString));
                e.Handled = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Url is not valid. Click the 'Edit Servers' button, and verify your DiscordUrl.", "Invalid URL");
            }
        }
    }
}
