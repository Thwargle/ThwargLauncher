using System;
using System.IO;
using System.Text;
using Mag.Shared;

namespace MagFilter
{
    public class FileLocations
    {
        internal static string PluginName = "Mag-Filter";

        public static string GetPluginSettingsFile()
        {
            return PluginPersonalFolder.FullName + @"\" + PluginName + ".xml";
        }
        /// <summary>
        /// Launch file contains instructions (character) name from ThwargLauncher.exe for Mag-Filter.dll
        /// </summary>
        public static string GetCurrentLaunchFilePath()
        {
            string path = Path.Combine(AppFolder, PluginName + "_launch.txt");
            return path;
        }
        /// <summary>
        /// Launch response file contains pid of game, given by Mag-Filter.dll to ThwargLauncher.exe
        /// </summary>
        public static string GetCurrentLaunchResponseFilePath()
        {
            string path = Path.Combine(AppFolder, PluginName + "_launchResponse.txt");
            return path;
        }

        public static string GetCharacterFilePath()
        {
            string path = Path.Combine(AppFolder, "characters.txt");
            return path;
        }

        /// <summary>
        /// Returns path to the folder where we store profiles
        /// creates it if it doesn't yet exist
        /// </summary>
        /// <returns></returns>
        private string GetRunningFolder()
        {
            string profilesFolder = Path.Combine(AppFolder, "Running");
            if (!Directory.Exists(profilesFolder))
            {
                Directory.CreateDirectory(profilesFolder);
            }
            return profilesFolder;
        }

        public static string AppFolder
        {
            get
            {
                // Combine the base folder with your specific folder....
                string specificFolder = System.IO.Path.Combine(AppBaseFolder, "ACAccountManager");
                return specificFolder;
            }
        }
        public static string AppBaseFolder
        {
            get
            {
                // The folder for the roaming current user 
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }


        internal static DirectoryInfo PluginPersonalFolder
        {
            get
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\Decal Plugins";
                DirectoryInfo pluginPersonalFolder = new DirectoryInfo(folder + @"\" + PluginName);

                try
                {
                    if (!pluginPersonalFolder.Exists)
                        pluginPersonalFolder.Create();
                }
                catch (Exception ex) { Debug.LogException(ex); }

                return pluginPersonalFolder;
            }
        }
    }
}
