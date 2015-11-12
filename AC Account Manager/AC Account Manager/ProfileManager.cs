using System;
using System.Collections.Generic;
using System.IO;

namespace AC_Account_Manager
{
    /// <summary>
    /// Class which handles actually reading and writing profile (json) string to & from disk
    /// </summary>
    class ProfileManager
    {
        public void EnsureProfileFolderExists()
        {
            GetProfileFolder(); // creates folder if needed
        }
        public void Save(Profile profile)
        {
            string filepath = GetProfileFilePath(profile.Name);
            using (StreamWriter stream = new StreamWriter(filepath))
            {
                string contents = profile.StoreToSerialized();
                stream.WriteLine(contents);
            }
        }
        public Profile Load(string profileName)
        {
            string filepath = GetProfileFilePath(profileName);
            if (!File.Exists(filepath)) { return null; }
            var profile = new Profile();
            using (StreamReader stream = new StreamReader(filepath))
            {
                string contents = stream.ReadToEnd();
                profile.LoadFromSerialized(contents);
            }
            profile.Name = profileName;
            return profile;
        }
        public Profile GetNextProfile(string profileName)
        {
            var allProfiles = GetAllProfiles();
            if (allProfiles.Count < 2) { return null; }
            int index = allProfiles.FindIndex(i => i.Name == profileName);
            ++index;
            index = (index%allProfiles.Count);
            return allProfiles[index];
        }
        public Profile GetPrevProfile(string profileName)
        {
            var allProfiles = GetAllProfiles();
            if (allProfiles.Count < 2) { return null; }
            int index = allProfiles.FindIndex(i => i.Name == profileName);
            --index;
            if (index < 0) { index += allProfiles.Count; }
            index = (index % allProfiles.Count);
            return allProfiles[index];
        }
        public List<Profile> GetAllProfiles()
        {
            var allProfiles = new List<Profile>();
            string profilesFolder = GetProfileFolder();
            DirectoryInfo dir = new DirectoryInfo(profilesFolder);
            var badProfiles = new List<string>();
            foreach (var fileInfo in dir.EnumerateFiles("*.txt"))
            {
                string profileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                try
                {
                    Profile profile = this.Load(profileName);
                    allProfiles.Add(profile);
                }
                catch
                {
                    badProfiles.Add(profileName);
                }
            }
            if (badProfiles.Count > 0)
            {
                string msg = string.Format(
                    "{0} unloadable profiles found: \r\n{1}",
                    badProfiles.Count,
                    string.Join(", ", badProfiles));
                msg += "\r\nDelete unloadable profile(s)?";
                string caption = string.Format("Profile errors: {0}", badProfiles.Count);
                var choice = System.Windows.MessageBox.Show(msg, caption, System.Windows.MessageBoxButton.YesNo);
                if (choice == System.Windows.MessageBoxResult.Yes)
                {
                    DeleteProfiles(badProfiles);
                }
            }
            allProfiles.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
            return allProfiles;
        }
        private void DeleteProfiles(List<string> profileNameList)
        {
            foreach (string profileName in profileNameList)
            {
                try
                {
                    DeleteProfile(profileName);
                }
                catch
                {
                }
            }
        }
        private void DeleteProfile(string profileName)
        {
            string filepath = GetProfileFilePath(profileName);
            File.Delete(filepath);
        }
        private string GetProfileFilePath(string profileName)
        {
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                if (profileName.Contains(Char.ToString(ch)))
                {
                    throw new Exception(string.Format(
                        "Invalid character '{0}' in profile name '{1}'",
                        ch, profileName));
                }
            }
            string profilesFolder = GetProfileFolder();
            string filename = string.Format("{0}.txt", profileName);
            string filepath = Path.Combine(profilesFolder, filename);
            return filepath;
        }
        private string GetProfileFolder()
        {
            string profilesFolder = Path.Combine(Configuration.AppFolder, "Profiles");
            if (!Directory.Exists(profilesFolder))
            {
                Directory.CreateDirectory(profilesFolder);
            }
            return profilesFolder;
        }
    }
}
