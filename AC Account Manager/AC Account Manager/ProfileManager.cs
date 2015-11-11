using System;
using System.Collections.Generic;
using System.IO;

namespace AC_Account_Manager
{
    class ProfileManager
    {
        public void Save(Profile profile, string profileName)
        {
            string filepath = GetProfileFilePath(profileName);
            using (StreamWriter stream = new StreamWriter(filepath))
            {
                stream.WriteLine("Version: 2");
                stream.WriteLine("Date: {0}", DateTime.UtcNow);
                foreach (var acctState in profile.EnumerateAccountStates())
                {
                    stream.WriteLine(
                        "{0},{1}",
                        acctState.AccountName,
                        acctState.Active
                        );
                }
                foreach (var setting in profile.EnumerateCharacterSettings())
                {
                    stream.WriteLine(
                        "{0},{1},{2},{3}",
                        setting.AccountName,
                        setting.ServerName,
                        setting.Active,
                        setting.ChosenCharacter);
                }
            }
        }
        public Profile Load(string profileName)
        {
            string filepath = GetProfileFilePath(profileName);
            if (!File.Exists(filepath)) { return null; }
            var profile = new Profile();
            using (StreamReader stream = new StreamReader(filepath))
            {
                string versionStr = stream.ReadLine();
                if (versionStr != "Version: 2") { return null; }
                string dateStr = stream.ReadLine();
                if (dateStr == null || !dateStr.StartsWith("Date: ")) { return null; }
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    char[] delimiterChars = { ',' };
                    string[] pieces = line.Split(delimiterChars);
                    if (pieces.Length == 4)
                    {
                        var charSetting = new CharacterSetting();
                        charSetting.AccountName = pieces[0];
                        charSetting.ServerName = pieces[1];
                        charSetting.Active = bool.Parse(pieces[2]);
                        charSetting.ChosenCharacter = pieces[3];
                        profile.StoreCharacterSetting(charSetting);
                    }
                    else if (pieces.Length == 2)
                    {
                        string accountName = pieces[0];
                        bool active = bool.Parse(pieces[1]);
                        profile.StoreAccountState(accountName: accountName, active: active);
                    }
                }
            }
            return profile;
        }
        private string GetProfileFilePath(string profileName)
        {
            string filename = string.Format("{0}.txt", profileName);
            string filepath = System.IO.Path.Combine(Configuration.AppFolder, filename);
            return filepath;
        }

    }
}
