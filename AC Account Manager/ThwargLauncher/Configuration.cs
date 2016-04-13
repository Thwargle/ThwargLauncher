using System;
using System.IO;

namespace ThwargLauncher
{
    class Configuration
    {
        public static string AppFolder
        {
            get
            {
                return MagFilter.FileLocations.AppFolder;
            }
        }
        public static string UserPreferencesFile
        {
            get
            {
                string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(mydocs, "Asheron's Call\\UserPreferences.ini");
            }
        }
        public static string UserPreferencesBaseFile
        {
            get
            {
                string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(mydocs, "Asheron's Call\\UserPreferences_base.ini");
            }
        }
    }
}
