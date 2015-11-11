using System;
using System.IO;

namespace AC_Account_Manager
{
    /// <summary>
    /// Class which handles actually reading and writing profile (json) string to & from disk
    /// </summary>
    class ProfileManager
    {
        public void Save(Profile profile, string profileName)
        {
            string filepath = GetProfileFilePath(profileName);
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
            return profile;
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
            string profilesFolder = Path.Combine(Configuration.AppFolder, "Profiles");
            if (!Directory.Exists(profilesFolder))
            {
                Directory.CreateDirectory(profilesFolder);
            }

            string filename = string.Format("{0}.txt", profileName);
            string filepath = Path.Combine(profilesFolder, filename);
            return filepath;
        }
    }
}
