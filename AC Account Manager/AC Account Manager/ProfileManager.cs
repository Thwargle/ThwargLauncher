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
                stream.WriteLine("Version: 1");
                stream.WriteLine("Date: {0}", DateTime.UtcNow);
                foreach (var setting in profile.EnumerateCharacterSettings())
                {
                    stream.WriteLine("{0},{1},{2}", setting.AccountName, setting.ServerName, setting.ChosenCharacter);
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
                if (versionStr != "Version: 1") { return null; }
                string dateStr = stream.ReadLine();
                if (dateStr == null || !dateStr.StartsWith("Date: ")) { return null; }
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    char[] delimiterChars = { ',' };
                    string[] pieces = line.Split(delimiterChars);
                    if (pieces.Length != 3) { break; }
                    profile.AddCharacterSetting(
                        accountName: pieces[0],
                        serverName: pieces[1],
                        chosenCharacter: pieces[2]
                        );
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
