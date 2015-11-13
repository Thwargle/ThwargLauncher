using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using CommonControls;

namespace AC_Account_Manager
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public void Reset()
        {
            KnownUserAccounts = new ObservableCollection<UserAccount>();
            SelectedUserAccountName = "";
        }
        public MainWindowViewModel()
        {
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
        }
        public void CreateNewProfile()
        {
            ProfileManager mgr = new ProfileManager();
            var newProfile = mgr.CreateNewProfile();
            GotoProfile(newProfile);
        }
        public void DeleteProfile()
        {
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
        public void GoToPrevProfile()
        {
            SaveCurrentProfile();
            ProfileManager mgr = new ProfileManager();
            var newProfile = mgr.GetPrevProfile(CurrentProfile.Name);
            if (newProfile != null)
            {
                GotoProfile(newProfile);
            }
        }
        private void GotoProfile(Profile profile)
        {
            Properties.Settings.Default.LastProfileName = profile.Name;
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
    }
}
