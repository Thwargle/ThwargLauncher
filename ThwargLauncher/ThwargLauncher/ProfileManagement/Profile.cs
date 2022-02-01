using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ThwargLauncher
{
    class Profile
    {
        [Serializable]
        public class AccountState
        {
            public string AccountName;
            public bool Active;
        }
        /// <summary>
        /// ProfileData is the data actually saved to disk in the profile file
        /// </summary>
        [Serializable]
        internal class ProfileData
        {
            public string FileVersion; // compared with CurrentVersion
            public string Name = "Default";
            public DateTime? LastActivatedDate;
            public DateTime? LastSavedDate;
            public DateTime? LastLaunchedDate;
            public string Description;
            public List<CharacterSetting> CharacterSettings;
            public List<AccountState> AccountStates;
        }
        private const string CurrentVersion = "VER-3.0";
        private readonly Dictionary<string, AccountState> _accountStates = new Dictionary<string, AccountState>();
        private ProfileData _profileData = new ProfileData();
        private readonly Dictionary<string, CharacterSetting> _characterSettings = new Dictionary<string, CharacterSetting>();
        public void ActivateProfile() { _profileData.LastActivatedDate = DateTime.UtcNow; }
        public DateTime? LastActivatedDate { get { return _profileData.LastActivatedDate; } }
        public DateTime? LastSavedDate { get { return _profileData.LastSavedDate; } }
        public DateTime? LastLaunchedDate { get { return _profileData.LastLaunchedDate; } set { _profileData.LastLaunchedDate = value; } }
        public string Name { get { return _profileData.Name; } set { _profileData.Name = value; } }
        public string Description
        {
            get { return _profileData.Description; }
            set { _profileData.Description = value; }
        }
        public int ActiveAccountCount
        {
            get
            {
                int count = 0;
                foreach (var acct in _accountStates.Values)
                {
                    if (acct.Active)
                    {
                        ++count;
                    }
                }
                return count;
            }
        }
        public int ActiveServerCount
        {
            get
            {
                int count = 0;
                foreach (var charset in _characterSettings.Values)
                {
                    if (charset.Active)
                    {
                        ++count;
                    }
                }
                return count;
            }
        }
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

        public override string ToString()
        {
            return _profileData.Name;
        }
        private string GetCharacterKey(string accountName, string serverName)
        {
            return accountName + ":" + serverName;
        }
        private string GetCharacterKey(CharacterSetting charSetting)
        {
            return GetCharacterKey(accountName: charSetting.AccountName, serverName: charSetting.ServerName);
        }
        public void StoreAccountState(string accountName, bool active)
        {
            string key = accountName;
            var accountState = new AccountState();
            accountState.AccountName = accountName;
            accountState.Active = active;
            _accountStates[key] = accountState;
        }
        public bool RetrieveAccountState(string accountName)
        {
            string key = accountName;
            if (_accountStates.ContainsKey(key))
            {
                return _accountStates[key].Active;
            }
            else
            {
                return false;
            }
            
        }
        /// <summary>
        /// Update our internal ProfileData object from our data
        /// (in preparation for saving to disk)
        /// </summary>
        private void PrepareToSaveProfileData()
        {
            _profileData.FileVersion = CurrentVersion;
            _profileData.LastSavedDate = DateTime.UtcNow;
            _profileData.CharacterSettings = this._characterSettings.Values.ToList();
            _profileData.AccountStates = this._accountStates.Values.ToList();
        }
        /// <summary>
        /// Store all data from this profile into a string of json
        /// </summary>
        public string StoreToSerialized()
        {
            PrepareToSaveProfileData();
            using (var stream1 = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(ProfileData));
                ser.WriteObject(stream1, _profileData);
                stream1.Position = 0;
                using (var reader = new StreamReader(stream1))
                {
                    string text = reader.ReadToEnd();
                    return text;
                }
            }
        }
        /// <summary>
        /// Read string of json and use it to populate this profile
        /// </summary>
        public void LoadFromSerialized(string text)
        {
            _profileData = Deserialize<ProfileData>(text);
            SanitizeDate(ref _profileData.LastActivatedDate);
            SanitizeDate(ref _profileData.LastLaunchedDate);
            SanitizeDate(ref _profileData.LastSavedDate);
            if (_profileData.FileVersion != CurrentVersion) { throw new Exception("Incompatible profile file"); }
            foreach (CharacterSetting setting in _profileData.CharacterSettings)
            {
                this.StoreCharacterSetting(setting);
            }
            foreach (AccountState state in _profileData.AccountStates)
            {
                this.StoreAccountState(state.AccountName, state.Active);
            }
        }
        private void SanitizeDate(ref DateTime? date)
        {
            if (date.HasValue && date.Value.Year < 2000)
            {
                date = null;
            }
        }
        public IList<string> GetAllServerNames()
        {
            var serverList = _characterSettings.Values.Select(qq => qq.ServerName).ToList();
            return serverList;
        }
        /// <summary>
        /// Read a string of json and build from it an object of type T
        /// To avoid JSON attack vulnerability, do not allow this type to be variable and under external control
        /// </summary>
        private static T Deserialize<T>(string json)
        {
            var instance = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(instance.GetType());
                return (T)serializer.ReadObject(ms);
            }
        }
    }
}
