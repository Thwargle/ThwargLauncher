using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommonControls;

namespace ThwargLauncher
{
    public delegate void HandleEvent();
    public delegate void LaunchGameDelegateMethod(LaunchItem launchItem);
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event HandleEvent OpeningSimpleLauncherEvent;
        public event LaunchGameDelegateMethod LaunchingSimpleGameEvent;
        public event HandleEvent RequestShowMainWindowEvent;
        public Action CloseAction { get; set; }
        public string ClientFileLocation
        {
            get
            {
                return GetACLocation();
            }
            set
            {
                if (Properties.Settings.Default.ACLocation != value)
                {
                    Properties.Settings.Default.ACLocation = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged("ClientFileLocation");
                }
            }
        }
        private string GetACLocation()
        {
            try
            {
                return Properties.Settings.Default.ACLocation;
            }
            catch
            {
                return @"C:\Turbine\Asheron's Call\acclient.exe";
            }
        }

        public bool AutoRelaunch
        {
            get { return GetAutoRelaunch(); }
            set
            {
                if (Properties.Settings.Default.AutoRelaunch != value)
                {
                    Properties.Settings.Default.AutoRelaunch = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged("AutoRelaunch");
                }
            }
        }
        public bool ShowCheckedAccounts
        {
            get { return GetShowCheckedAccounts(); }
            set
            {
                if (Properties.Settings.Default.ShowCheckedAccounts != value)
                {
                    Properties.Settings.Default.ShowCheckedAccounts = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged("KnownUserAccounts");
                }
            }
        }
        private bool GetAutoRelaunch()
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
        private bool GetShowCheckedAccounts()
        {
            try
            {
                return Properties.Settings.Default.ShowCheckedAccounts;
            }
            catch
            {
                return false;
            }
        }

        private AccountManager _accountManager;
        private GameSessionMap _gameSessionMap;
        private Configurator _configurator;
        HelpWindow _helpWindow = null;
        LogViewerViewModel _logViewerViewmodel = new LogViewerViewModel();
        LogViewerWindow _logViewer = null;
        SimpleLaunchWindow _simpleLaunchWindow = null;
        SimpleLaunchWindowViewModel _simpleLaunchViewModel = null;
        private bool _switchingToMainWindow = false;
        private ObservableCollection<UserAcctViewModel> _userAccountViewModels = new ObservableCollection<UserAcctViewModel>();

        public MainWindowViewModel(AccountManager accountManager, GameSessionMap gameSessionMap, Configurator configurator)
        {
            if (accountManager == null) { throw new ArgumentException("Null Null GameSessionMap in MainWindowViewModel()", "accountManager"); }
            if (gameSessionMap == null) { throw new ArgumentException("Null GameSessionMap in MainWindowViewModel()", "gameSessionMap"); }
            if (configurator == null) { throw new ArgumentException("Null Configurator in MainWindowViewModel()", "configurator"); }

            _accountManager = accountManager;
            _gameSessionMap = gameSessionMap;
            _configurator = configurator;

            _accountManager.UserAccounts.CollectionChanged += UserAccountsCollectionChanged;
            _accountManager.SomeAccountLaunchableChangedEvent += accountManager_SomeAccountLaunchableChangedEvent;

            NewProfileCommand = new DelegateCommand(CreateNewProfile);
            NextProfileCommand = new DelegateCommand(GoToNextProfile);
            PrevProfileCommand = new DelegateCommand(GoToPrevProfile);
            DeleteProfileCommand = new DelegateCommand(DeleteProfile);
            EditCharactersCommand = new DelegateCommand(EditCharacters);
            LoadStatusSymbols();
        }

        private void accountManager_SomeAccountLaunchableChangedEvent(object sender, EventArgs e)
        {
            OnPropertyChanged("KnownUserAccounts");
        }

        void UserAccountsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                _userAccountViewModels.Clear();
                return;
            }
            if (e.OldItems != null)
            {
                foreach (UserAccount ua in e.OldItems)
                {
                    UserAcctViewModel avm = _userAccountViewModels.FirstOrDefault(vm => vm.Account == ua);
                    if (avm != null)
                    {
                        _userAccountViewModels.Remove(avm);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (UserAccount ua in e.NewItems)
                {
                    UserAcctViewModel avm = new UserAcctViewModel(ua);
                    _userAccountViewModels.Add(avm);
                }
            }
        }
        private void CreateNewProfile()
        {
            ProfileManager mgr = new ProfileManager();
            var newProfile = mgr.CreateNewProfile();
            GotoProfile(newProfile);
        }
        private void DeleteProfile()
        {
            //Confirm Delete
            if (MessageBox.Show("Are you sure you want to delete the " + CurrentProfileName + " profile?", "Confirm Delete", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;

            DeleteProfileByName(CurrentProfile.Name);
        }
        public void DeleteProfileByName(string profileName)
        {
            ProfileManager mgr = new ProfileManager();
            var nextProfile = mgr.GetPrevProfile(profileName);

            if (nextProfile == null)
            {
                nextProfile = mgr.CreateNewProfile();
            }

            var profileNametoDelete = profileName;

            GotoProfile(nextProfile);
            mgr.DeleteProfile(profileNametoDelete);
        }
        public void GoToNextProfile()
        {
            SaveCurrentProfile();
            ProfileManager mgr = new ProfileManager();
            var newProfile = mgr.GetNextProfile(CurrentProfile.Name);
            if (newProfile != null)
            {
                GotoProfile(newProfile);
            }
        }
        private void GoToPrevProfile()
        {
            SaveCurrentProfile();
            ProfileManager mgr = new ProfileManager();
            var newProfile = mgr.GetPrevProfile(CurrentProfile.Name);
            if (newProfile != null)
            {
                GotoProfile(newProfile);
            }
        }
        public void GotoSpecificProfile(string profileName)
        {
            GotoProfileByName(profileName);
        }
        private void GotoProfile(Profile profile)
        {
            GotoProfileByName(profile.Name);
        }
        private void GotoProfileByName(string profileName)
        {
            Properties.Settings.Default.LastProfileName = profileName;
            Properties.Settings.Default.Save();
            LoadMostRecentProfile();
        }
        public void RecordProfileLaunch()
        {
            CurrentProfile.LastLaunchedDate = DateTime.UtcNow;
            SaveCurrentProfile();
        }

        public ObservableCollection<UserAcctViewModel> KnownUserAccounts
        {
            get
            {
                if (GetShowCheckedAccounts())
                {
                    if (CurrentProfile == null) { return new ObservableCollection<UserAcctViewModel>(); }
                    var accounts = _userAccountViewModels.Where(a => a.AccountLaunchable).ToList();
                    return new ObservableCollection<UserAcctViewModel>(accounts);
                }
                else
                {
                    return _userAccountViewModels;
                }
            }
        }
        public string SelectedUserAccountName { get; set; }
        private Profile CurrentProfile { get; set; }
        public string CurrentProfileName
        {
            get { return (CurrentProfile != null ? CurrentProfile.Name : ""); }
            set {
                if (CurrentProfile.Name != value)
                {
                    SaveCurrentProfile();
                    var profileManager = new ProfileManager();

                    bool renamed = profileManager.RenameProfile(CurrentProfile.Name, value);

                    if (renamed)
                    {
                        CurrentProfile.Name = value;
                        OnPropertyChanged("CurrentProfileName");
                    }
                }
            }
        }
        public ICommand NextProfileCommand { get; private set; }
        public ICommand PrevProfileCommand { get; private set; }
        public ICommand NewProfileCommand { get; private set; }
        public ICommand DeleteProfileCommand { get; private set; }
        public ICommand EditCharactersCommand { get; private set; }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void ApplyCurrentProfileToModel()
        {
            foreach (var account in KnownUserAccounts)
            {
                account.AccountLaunchable = CurrentProfile.RetrieveAccountState(account.AccountName); ;
                foreach (var server in account.Servers)
                {
                    var charSetting = CurrentProfile.RetrieveCharacterSetting(accountName: account.AccountName, serverName: server.ServerName);
                    if (charSetting != null)
                    {
                        var state = _gameSessionMap.GetGameSessionStateByServerAccount(server.ServerName, account.AccountName);
                        string statusSymbol = GetStatusSymbol(state);
                        server.SetAccountServerStatus(state, statusSymbol);
                        server.ServerSelected = charSetting.Active;
                        server.ChosenCharacter = charSetting.ChosenCharacter;
                        if (string.IsNullOrEmpty(server.ChosenCharacter))
                        {
                            server.ChosenCharacter = "None";
                        }
                    }
                }
            }
            OnPropertyChanged("KnownUserAccounts");
        }
        public void ReloadCharacters()
        {
            _accountManager.ReloadCharacters();
        }
        public void ReloadAccounts(string oldUsersFilePath)
        {
            SelectedUserAccountName = "";
            _accountManager.ReloadAccounts(oldUsersFilePath);
        }
        public void UpdateProfileFromCurrentModelSettings()
        {
            foreach (var account in this.KnownUserAccounts)
            {
                CurrentProfile.StoreAccountState(account.AccountName, account.AccountLaunchable);
                foreach (var server in account.Servers)
                {
                    var charSetting = new CharacterSetting();
                    charSetting.AccountName = account.AccountName;
                    charSetting.ServerName = server.ServerName;
                    charSetting.Active = server.ServerSelected;
                    charSetting.ChosenCharacter = server.ChosenCharacter;
                    CurrentProfile.StoreCharacterSetting(charSetting);
                }
            }
        }
        public void ReloadCurrentProfile()
        {
            SaveCurrentProfile();
            LoadMostRecentProfile();
        }
        public void LoadMostRecentProfile()
        {
            string profileName = GetLastProfileName();
            LoadProfile(profileName);
        }
        private string GetLastProfileName()
        {
            try
            {
                string profileName = Properties.Settings.Default.LastProfileName;
                return profileName;
            }
            catch
            {
                return null;
            }
        }
        private void LoadProfile(string profileName)
        {
            string previousProfileName = CurrentProfileName;
            ProfileManager mgr = new ProfileManager();
            try
            {
                if (string.IsNullOrEmpty(profileName))
                {
                    profileName = "Default";
                }
                CurrentProfile = mgr.Load(profileName);
                if (CurrentProfile == null && profileName != "Default")
                {
                    profileName = "Default";
                    CurrentProfile = mgr.Load(profileName);
                }
                CurrentProfile.ActivateProfile();
            }
            catch
            {
                if (CurrentProfile == null)
                {
                    CurrentProfile = new Profile();
                }
            }
            finally
            {
                if (CurrentProfile == null)
                {
                    CurrentProfile = new Profile();
                }
            }
            ApplyCurrentProfileToModel();
            if (previousProfileName != CurrentProfileName)
            {
                OnPropertyChanged("CurrentProfileName");
            }
        }
        public void WindowClosing()
        {
            SaveCurrentProfile();
            foreach (var acct in _userAccountViewModels)
            {
                acct.SaveSettings();
            }
            var settings = PersistenceHelper.SettingsFactory.Get();
            settings.Save();
        }
        public void SaveCurrentProfile()
        {
            UpdateProfileFromCurrentModelSettings();

            var mgr = new ProfileManager();
            mgr.Save(CurrentProfile);
            Properties.Settings.Default.LastProfileName = CurrentProfile.Name;
            Properties.Settings.Default.Save();
        }
        public void CreateProfileIfDoesNotExist()
        {
            ProfileManager mgr = new ProfileManager();
            string profileName = CurrentProfileName;
            if (string.IsNullOrEmpty(profileName))
            {
                profileName = "Default";
            }
            mgr.CreateProfileIfDoesNotExist(profileName);
        }

        internal void UpdateAccountStatus(string serverName, string accountName, ServerAccountStatusEnum status)
        {
            AccountServer acctServer = FindServer(serverName, accountName);
            if (acctServer != null)
            {
                string symbol = GetStatusSymbol(status);
                Server server = acctServer.tServer;
                server.SetAccountServerStatus(status, symbol);
                if (DateTime.UtcNow - server.LastStatusSummaryChangedNoticeUtc > TimeSpan.FromSeconds(2.0))
                {
                    server.NotifyOfStatusSummaryChanged();
                }
                acctServer.tAccount.NotifyAccountSummaryChanged();
            }
        }
        internal void ExecuteGameCommand(string serverName, string accountName, string command)
        {
            AccountServer acctServer = FindServer(serverName, accountName);
            if (acctServer == null) { return; }
            Logger.WriteInfo(string.Format(
                "QQQ - not currently invoked -- Command received from server='{0}', account='{1}': {2}",
                serverName, accountName, command));
            // TODO
            // write code to implement commands from game to launcher
            if (acctServer != null)
            {
            }
            else
            {
                Logger.WriteError("Command received from unknown server/account");
            }
        }
        class AccountServer { public Server tServer; public UserAccount tAccount; }
        private AccountServer FindServer(string serverName, string accountName)
        {
            var account = KnownUserAccounts.FirstOrDefault(x => x.AccountName == accountName);
            if (account == null) { return null; }
            var server = account.Servers.FirstOrDefault(x => x.ServerName == serverName);
            if (server == null) { return null; }
            AccountServer acctServer = new AccountServer() { tAccount = account.Account, tServer = server };
            return acctServer;
        }
        public void NotifyAvailableCharactersChanged()
        {
            foreach (var userAcct in KnownUserAccounts)
            {
                foreach (var server in userAcct.Servers)
                {
                    server.NotifyAvailableCharactersChanged();
                }
            }
        }
        private string _SessionStatusNone;
        private string _SessionStatusStarting;
        private string _SessionStatusRunning;
        private string _SessionStatusWarning;
        private void LoadStatusSymbols()
        {
            _SessionStatusNone = ConfigSettings.GetConfigString("SessionStatusNone", "🎻");
            _SessionStatusStarting = ConfigSettings.GetConfigString("SessionStatusStarting", "=");
            _SessionStatusRunning = ConfigSettings.GetConfigString("SessionStatusRunning", "✔");
            _SessionStatusWarning = ConfigSettings.GetConfigString("SessionStatusWarning", "☔");

        }
        private string GetStatusSymbol(ServerAccountStatusEnum status)
        {
            switch (status)
            {
                case ServerAccountStatusEnum.None: return _SessionStatusNone;
                case ServerAccountStatusEnum.Starting: return _SessionStatusStarting;
                case ServerAccountStatusEnum.Running: return _SessionStatusRunning;
                case ServerAccountStatusEnum.Warning: return _SessionStatusWarning;
                default:
                    return "✖";
            }
        }
        public void DisplayHelpWindow()
        {
            try
            {
                if (_helpWindow == null)
                {
                    var hwvm = new HelpWindowViewModel(_configurator);
                    _helpWindow = new HelpWindow(hwvm);
                    _helpWindow.Closing += (s, e) => _helpWindow = null;
                }
                _helpWindow.Activate();
                _helpWindow.Show();
            }
            catch (Exception exc)
            {
                Logger.WriteError("DisplayHelpWindow exception: {0}", exc);
            }
        }
        public void DisplaySimpleLauncher()
        {
            if (_simpleLaunchWindow == null)
            {
                if (_simpleLaunchViewModel == null)
                {
                    _simpleLaunchViewModel = SimpleLaunchWindowViewModel.CreateViewModel();
                    _simpleLaunchViewModel.LaunchingEvent += OnRequestExecuteSimpleLaunch;
                    _simpleLaunchViewModel.RequestingMainViewEvent += OnSimpleLaunchRequestMainView;
                    _simpleLaunchViewModel.RequestingConfigureFileLocationEvent += OnSimpleLaunchRequestConfigureFileLocation;
                }
                _simpleLaunchWindow = new SimpleLaunchWindow(_simpleLaunchViewModel);
                _simpleLaunchWindow.Closing += OnSimpleLaunchWindowClosing;
            }
            try
            {
                Properties.Settings.Default.LastUsedSimpleLaunch = true;
            }
            catch
            {
            }
            _simpleLaunchWindow.Show();
            if (OpeningSimpleLauncherEvent != null)
                OpeningSimpleLauncherEvent();
        }

        private void OnSimpleLaunchRequestConfigureFileLocation(object sender, EventArgs e)
        {
            ChooseLauncherLocation();
        }

        void OnSimpleLaunchRequestMainView(object sender, EventArgs e)
        {
            RequestShowMainWindow();
        }
        public void CallSimpleLauncher()
        {
            DisplaySimpleLauncher();
        }

        void OnSimpleLaunchWindowClosing(object sender, CancelEventArgs e)
        {
            _simpleLaunchWindow = null;
            if (!_switchingToMainWindow)
            {
                if (CloseAction == null) { throw new Exception("Null CloseAction in OnSimpleLaunchWindowClosing"); }
                CloseAction();
            }
        }
        private void RequestShowMainWindow()
        {
            Properties.Settings.Default.LastUsedSimpleLaunch = false;
            _switchingToMainWindow = true;
            if (_simpleLaunchViewModel != null)
            {
                _simpleLaunchViewModel.CloseAction();
            }
            if (RequestShowMainWindowEvent != null)
            {
                RequestShowMainWindowEvent();
            }
            _switchingToMainWindow = false;
        }
        private void EditCharacters()
        {
            var vm = new AccountManagement.EditCharactersViewModel(_accountManager);
            var win = new AccountManagement.EditCharactersWindow(vm);
            win.Show();
        }

        void OnRequestExecuteSimpleLaunch(LaunchItem launchItem)
        {
            if (LaunchingSimpleGameEvent == null) { throw new Exception("MainWindowViewModel null LaunchingSimpleGameEvent"); }
            LaunchingSimpleGameEvent(launchItem);
        }

        public void DisplayLogWindow()
        {
            if (_logViewer == null)
            {
                _logViewer = new LogViewerWindow(_logViewerViewmodel);
                _logViewer.Closing += _logViewer_Closing;
            }
            _logViewer.Show();
        }

        void _logViewer_Closing(object sender, CancelEventArgs e)
        {
            _logViewer = null;
        }
        public void ShutSubsidiaryWindows()
        {
            if (_logViewer != null)
            {
                _logViewer.Close();
                _logViewer = null;
            }
            if (_helpWindow != null)
            {
                _helpWindow.Close();
                _helpWindow = null;
            }
        }
        public void ChooseLauncherLocation()
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
                ClientFileLocation = dlg.FileName;
            }
        }
    }
}
