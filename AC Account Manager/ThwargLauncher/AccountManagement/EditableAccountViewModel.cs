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

        public bool LocalEnabled
        {
            get { return IsServerEnabled("Local"); }
            set { SetServerEnabled("Local", value); }
        }
        public bool FrostfellEnabled
        {
            get { return IsServerEnabled("Frostfell"); }
            set { SetServerEnabled("Frostfell", value); }
        }
        public bool HarvestgainEnabled
        {
            get { return IsServerEnabled("Harvestgain"); }
            set { SetServerEnabled("Harvestgain", value); }
        }
        public bool LeafcullEnabled
        {
            get { return IsServerEnabled("Leafcull"); }
            set { SetServerEnabled("Leafcull", value); }
        }
        public bool MorningthawEnabled
        {
            get { return IsServerEnabled("Morningthaw"); }
            set { SetServerEnabled("Morningthaw", value); }
        }
        public bool SolclaimEnabled
        {
            get { return IsServerEnabled("Solclaim"); }
            set { SetServerEnabled("Solclaim", value); }
        }
        public bool ThistledownEnabled
        {
            get { return IsServerEnabled("Thistledown"); }
            set { SetServerEnabled("Thistledown", value); }
        }
        public bool VerdantineEnabled
        {
            get { return IsServerEnabled("Verdantine"); }
            set { SetServerEnabled("Verdantine", value); }
        }
        public bool WintersEbbEnabled
        {
            get { return IsServerEnabled("WintersEbb"); }
            set { SetServerEnabled("WintersEbb", value); }
        }
        public bool DarktideEnabled
        {
            get { return IsServerEnabled("Darktide"); }
            set { SetServerEnabled("Darktide", value); }
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
