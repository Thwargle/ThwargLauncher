using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    internal class AccountManager
    {
        public ObservableCollection<UserAccount> UserAccounts = new ObservableCollection<UserAccount>();

        public delegate void SomeAccountLaunchableChanged(object sender, EventArgs e);
        public event SomeAccountLaunchableChanged SomeAccountLaunchableChangedEvent;

        private GameMonitor _gameMonitor;
        private object _locker = new object();

        public AccountManager(GameMonitor gameMonitor)
        {
            _gameMonitor = gameMonitor;
            _gameMonitor.CharacterFileChanged += _gameMonitor_CharacterFileChanged;
        }

        void _gameMonitor_CharacterFileChanged()
        {
            ReloadCharacters();
        }
        public void ReloadAccounts(string oldUsersFilePath)
        {
            AccountParser parser = new AccountParser();
            List<UserAccount> accounts = null;
            try
            {
                accounts = parser.ReadOrMigrateAccounts(oldUsersFilePath);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Exception reading account file: " + exc.Message);
                accounts = new List<UserAccount>();
            }
            lock (_locker)
            {
                UserAccounts.Clear();
                foreach (UserAccount acct in accounts)
                {
                    UserAccounts.Add(acct);
                    acct.PropertyChanged += Acct_PropertyChanged;
                }
            }
        }
        private void Acct_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AccountLaunchable")
            {
                if (SomeAccountLaunchableChangedEvent != null)
                {
                    SomeAccountLaunchableChangedEvent(sender, e);
                }
            }
        }
        public void ReloadCharacters()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ReloadCharactersOnPrimaryThead();
            }));
        }
        private void ReloadCharactersOnPrimaryThead()
        {
            var charBook = ThwargFilter.CharacterBook.ReadCharacters();
            if (charBook == null) { return;  }
            lock (_locker)
            {
                foreach (var uacct in UserAccounts)
                {
                    foreach (var srvr in uacct.Servers)
                    {
                        var currentNameList = srvr.AvailableCharacters.Where(x => x.Id != 0).Select(x => x.Name).ToList();
                        currentNameList.Sort();
                        var newMagDataList = charBook.GetCharactersOrEmpty(srvr.ServerName, uacct.Name);
                        var newNameList = newMagDataList.CharacterList.Select(x => x.Name).ToList();
                        newNameList.Sort();
                        if (!currentNameList.SequenceEqual(newNameList))
                        {
                            uacct.LoadCharacterListFromThwargFilterData(srvr, newMagDataList);
                            srvr.NotifyAvailableCharactersChanged();
                        }
                    }
                }
            }
        }
    }
}
