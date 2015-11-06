using System;

namespace AC_Account_Manager
{
    class Configuration
    {
        public static string AppBaseFolder 
        {
            get
            {
                // The folder for the roaming current user 
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
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
    }
}
