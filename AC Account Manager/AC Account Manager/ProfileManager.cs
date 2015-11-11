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
        public List<Profile> GetAllProfiles()
        {
            var allProfiles = new List<Profile>();
            string profilesFolder = GetProfileFolder();
            DirectoryInfo dir = new DirectoryInfo(profilesFolder);
            var errors = new List<string>();
            foreach (var fileInfo in dir.EnumerateFiles("*.txt"))
            {
                try
                {
                    string profileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    Profile profile = this.Load(profileName);
                    allProfiles.Add(profile);
                }
                catch
                {
                    errors.Add(string.Format("Error trying to open profile file: {0}", fileInfo.Name));
                }
            }
            if (errors.Count > 0)
            {
                string msg = string.Join(", ", errors);
                string caption = string.Format("Profile failures: {0}", errors.Count);
                System.Windows.MessageBox.Show(msg, caption);
            }
            return allProfiles;
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
