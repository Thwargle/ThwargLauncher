using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ThwargLauncher.AccountManagement
{
    class AccountEditorViewModel
    {
        private readonly ObservableCollection<EditableAccountViewModel> _accountViewModels = new ObservableCollection<EditableAccountViewModel>();
        private readonly IEnumerable<UserAccount> _accounts = null;
        public ObservableCollection<EditableAccountViewModel> AllAccounts { get { return _accountViewModels; } }
        public AccountEditorViewModel(IEnumerable<UserAccount> accounts)
        {
            _accounts = accounts;
            foreach (var account in accounts)
            {
                var accountModel = new EditableAccountViewModel(account);
                _accountViewModels.Add(accountModel);
            }
        }
        public void StoreToDisk()
        {
            AccountParser parser = new AccountParser();
            parser.WriteAccounts(_accounts);
        }
    }
}
