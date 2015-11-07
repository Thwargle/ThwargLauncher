using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace AC_Account_Manager
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public void Reset()
        {
            KnownUserAccounts = new List<UserAccount>();
            SelectedUserAccountName = "";
        }
        public List<UserAccount> KnownUserAccounts { get; set; }
        public string SelectedUserAccountName { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
