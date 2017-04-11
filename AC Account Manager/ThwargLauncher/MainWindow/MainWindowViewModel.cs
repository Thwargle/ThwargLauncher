using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private GameSessionMap _gameSessionMap;
        private Configurator _configurator;
        HelpWindow _helpWindow = null;
        LogViewerViewModel _logViewerViewmodel = new LogViewerViewModel();
        LogViewerWindow _logViewer = null;
        SimpleLaunchWindow _simpleLaunchWindow = null;
        SimpleLaunchWindowViewModel _simpleLaunchViewModel = null;
        private bool _switchingToMainWindow = false;

        public void Reset()
        {
            KnownUserAccounts = new ObservableCollection<UserAccount>();
            SelectedUserAccountName = "";
        }
        public MainWindowViewModel(GameSessionMap gameSessionMap, Configurator configurator)
        {
            if (gameSessionMap == null) { throw new Exception("Null GameSessionMap in MainWindowViewModel()"); }
            if (configurator == null) { throw new Exception("Null Configurator in MainWindowViewModel()"); }

            _gameSessionMap = gameSessionMap;
            _configurator = configurator;
            NewProfileCommand = new DelegateCommand(
                    CreateNewProfile
                );
            NextProfileCommand = new DelegateCommand(
                    GoToNextProfile
                );
            PrevProfileCommand = new DelegateCommand(
                    GoToPrevProfile
                );
            DeleteProfileCommand = new DelegateCommand(
                    DeleteProfile
                );
            LoadStatusSymbols();
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
            
            ProfileManager mgr = new ProfileManager();
            var nextProfile = mgr.GetPrevProfile(CurrentProfile.Name);

            if(nextProfile == null)
            {
                nextProfile = mgr.CreateNewProfile();
            }

            var profileNametoDelete = CurrentProfile.Name;

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

        public ObservableCollection<UserAccount> KnownUserAccounts { get; set; }
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

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void ApplyCurrentProfileToModel()
        {
            foreach (var account in this.KnownUserAccounts)
            {
                account.AccountLaunchable = CurrentProfile.RetrieveAccountState(account.Name); ;
                foreach (var server in account.Servers)
                {
                    var charSetting = CurrentProfile.RetrieveCharacterSetting(accountName: account.Name, serverName: server.ServerName);
                    if (charSetting != null)
                    {
                        var state = _gameSessionMap.GetGameSessionStateByServerAccount(server.ServerName, account.Name);
                        string statusSymbol = GetStatusSymbol(state);
                        server.ServerStatusSymbol = statusSymbol;
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
        public void UpdateProfileFromCurrentModelSettings()
        {
            foreach (var account in this.KnownUserAccounts)
            {
                CurrentProfile.StoreAccountState(account.Name, account.AccountLaunchable);
                foreach (var server in account.Servers)
                {
                    var charSetting = new CharacterSetting();
                    charSetting.AccountName = account.Name;
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
            string profileName = Properties.Settings.Default.LastProfileName;
            LoadProfile(profileName);
        }
        private void LoadProfile(string profileName)
        {
            string previousProfileName = CurrentProfileName;
            ProfileManager mgr = new ProfileManager();
            try
            {
                if (profileName == null)
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
                acctServer.tServer.ServerStatusSymbol = GetStatusSymbol(status);
                acctServer.tAccount.NotifyAccountSummaryChanged();
            }
        }
        internal void ExecuteGameCommand(string serverName, string accountName, string command)
        {
            AccountServer acctServer = FindServer(serverName, accountName);
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
            foreach (var account in KnownUserAccounts)
            {
                if (account.Name == accountName)
                {
                    var server = account.Servers.Find(x => x.ServerName == serverName);
                    AccountServer acctServer = new AccountServer() { tAccount = account, tServer = server };
                    return acctServer;
                }
            }
            return null;
        }
        public void NotifyVisibleServersChanged()
        {
            foreach (var userAcct in KnownUserAccounts)
            {
                userAcct.NotifyVisibleServersChanged();
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
            if (_helpWindow == null)
            {
                var hwvm = new HelpWindowViewModel(_configurator);
                hwvm.OpeningSimpleLauncherEvent += OnSimpleLauncher;
                _helpWindow = new HelpWindow(hwvm);
                _helpWindow.Closing += (s,e) => _helpWindow = null;
            }
            _helpWindow.Show();
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
                }
                _simpleLaunchWindow = new SimpleLaunchWindow(_simpleLaunchViewModel);
                _simpleLaunchWindow.Closing += OnSimpleLaunchWindowClosing;
            }
            Properties.Settings.Default.LastUsedSimpleLaunch = true;
            _simpleLaunchWindow.Show();
            if (OpeningSimpleLauncherEvent != null)
                OpeningSimpleLauncherEvent();
        }

        void OnSimpleLaunchRequestMainView(object sender, EventArgs e)
        {
            RequestShowMainWindow();
        }
        private void OnSimpleLauncher()
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
    }
}
