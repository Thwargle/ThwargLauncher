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
            KnownUserAccounts = new List<UserAccount>();
            SelectedUserAccountName = "";
        }
        public MainWindowViewModel()
        {
            NextProfileCommand = new DelegateCommand(
                    GoToNextProfile
                );
            PrevProfileCommand = new DelegateCommand(
                    GoToPrevProfile
                );
        }

        public void GoToNextProfile()
        {
            // TODO
            ProfileManager mgr = new ProfileManager();
            var allProfiles = mgr.GetAllProfiles();
            if (allProfiles.Count > 1)
            {
                if (allProfiles[0].Name != CurrentProfile.Name)
                {
                    CurrentProfile = allProfiles[0];
                }
                else
                {
                    CurrentProfile = allProfiles[1];
                }
                OnPropertyChanged("CurrentProfileName");
            }
            // ----
        }
        public void GoToPrevProfile()
        {
            // TODO
            GoToNextProfile();
        }

        public List<UserAccount> KnownUserAccounts { get; set; }
        public string SelectedUserAccountName { get; set; }
        private Profile CurrentProfile { get; set; }
        public string CurrentProfileName { get { return CurrentProfile.Name; } set { CurrentProfile.Name = value; } }
        public ICommand NextProfileCommand { get; set; }
        public ICommand PrevProfileCommand { get; set; }

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
        public void LoadProfile()
        {
            ProfileManager mgr = new ProfileManager();
            try
            {
                string profileName = Properties.Settings.Default.LastProfileName;
                if (string.IsNullOrWhiteSpace(profileName))
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
        }
        public void SaveCurrentProfile()
        {
            var mgr = new ProfileManager();
            mgr.Save(CurrentProfile);
            Properties.Settings.Default.LastProfileName = CurrentProfile.Name;
            Properties.Settings.Default.Save();
        }
    }
}
