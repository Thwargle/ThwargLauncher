using System;
using System.Collections.Generic;
using System.Text;

namespace AC_Account_Manager
{
    class Profile
    {
        private readonly Dictionary<string, CharacterSetting> _characterSettings = new Dictionary<string, CharacterSetting>();
        public void StoreCharacterSetting(CharacterSetting charSetting)
        {
            string key = GetCharacterKey(charSetting);
            _characterSettings[key] = charSetting;
        }
        public CharacterSetting RetrieveCharacterSetting(string accountName, string serverName)
        {
            string key = GetCharacterKey(accountName: accountName, serverName: serverName);
            if (_characterSettings.ContainsKey(key))
            {
                return _characterSettings[key];
            }
            else
            {
                return null;
            }
        }
        public IEnumerable<CharacterSetting> EnumerateCharacterSettings()
        {
            return _characterSettings.Values;
        }
        private string GetCharacterKey(string accountName, string serverName)
        {
            return accountName + ":" + serverName;
        }
        private string GetCharacterKey(CharacterSetting charSetting)
        {
            return GetCharacterKey(accountName: charSetting.AccountName, serverName: charSetting.ServerName);
        }
        public class AccountState { public string AccountName; public bool Active; }
        private readonly Dictionary<string, AccountState> _accountStates = new Dictionary<string, AccountState>();
        public void StoreAccountState(string accountName, bool active)
        {
            string key = accountName;
            var accountState = new AccountState();
            accountState.AccountName = accountName;
            accountState.Active = active;
            _accountStates[key] = accountState;
        }
        public AccountState RetrieveAccountState(string accountName)
        {
            string key = accountName;
            if (_accountStates.ContainsKey(key))
            {
                return _accountStates[key];
            }
            else
            {
                return null;
            }
            
        }
        public IEnumerable<AccountState> EnumerateAccountStates()
        {
            return _accountStates.Values;
        }
    }
}
