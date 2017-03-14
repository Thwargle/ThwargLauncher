using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher.AccountManagement
{
    public class EditableAccountViewModel
    {
        private readonly UserAccount _account;
        public EditableAccountViewModel(UserAccount account)
        {
            _account = account;
        }

        public string Name
        {
            get { return _account.Name; }
            set { _account.Name = value; }
        }
        public string Alias
        {
            get { return _account.Alias; }
            set { _account.Alias = value; }
        }
        public string Priority
        {
            get { return GetStringProperty("Priority"); }
            set { SetStringProperty("Priority", value); }
        }
        
        private string GetStringProperty(string key)
        {
            return _account.GetPropertyByName(key);
        }
        private void SetStringProperty(string key, string value)
        {
            _account.SetPropertyByName(key, value);
        }
        private bool IsServerEnabled(string serverName)
        {
            return _account.IsServerEnabled(serverName);
        }
        private void SetServerEnabled(string serverName, bool value)
        {
            _account.SetServerEnabled(serverName, value);
        }
    }
}
