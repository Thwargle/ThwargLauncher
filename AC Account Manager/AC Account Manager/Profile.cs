using System;
using System.Collections.Generic;
using System.Text;

namespace AC_Account_Manager
{
    class Profile
    {
        private readonly Dictionary<string, CharacterSetting> _characterSettings = new Dictionary<string, CharacterSetting>();
        public void AddCharacterSetting(string accountName, string serverName, string chosenCharacter)
        {
            string key = GetCharacterKey(accountName: accountName, serverName: serverName);
            var setting = new CharacterSetting
                {
                    AccountName = accountName,
                    ServerName = serverName,
                    ChosenCharacter = chosenCharacter
                };
            _characterSettings[key] = setting;
        }
        public string GetCharacterSetting(string accountName, string serverName)
        {
            string key = GetCharacterKey(accountName: accountName, serverName: serverName);
            if (_characterSettings.ContainsKey(key))
            {
                return _characterSettings[key].ChosenCharacter;
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
            return accountName.ToUpper() + ":" + serverName.ToUpper();
        }
    }
}
