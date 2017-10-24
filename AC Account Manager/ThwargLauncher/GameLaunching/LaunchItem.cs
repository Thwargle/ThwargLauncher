using System;

namespace ThwargLauncher
{
    /// <summary>
    /// Info for one game launch
    /// </summary>
    public class LaunchItem
    {
        public string AccountName;
        public string Priority;
        public string Password;
        public string IpAndPort;
        public string ServerName;
        public ServerModel.ServerEmuEnum EMU;
        public string CharacterSelected;
        public ServerModel.RodatEnum RodatSetting;
        public ServerModel.SecureEnum SecureSetting;
        public string CustomLaunchPath;
        public string CustomPreferencePath;
        public bool IsSimpleLaunch;
    }
}
