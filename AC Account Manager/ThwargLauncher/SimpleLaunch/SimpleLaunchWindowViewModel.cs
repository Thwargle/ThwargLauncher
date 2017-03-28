using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ThwargLauncher
{
    public class SimpleLaunchWindowViewModel : INotifyPropertyChanged
    {
        public event LaunchGameDelegateMethod LaunchingEvent;
        public static SimpleLaunchWindowViewModel CreateViewModel()
        {
            var vmodel = new SimpleLaunchWindowViewModel();
            return vmodel;
        }
        private SimpleLaunchWindowViewModel()
        {
            _servers = new CollectionView(ServerManager.ServerList);
            LoadFromSettings();
        }
        public void LoadFromSettings()
        {
            UseDecal = Properties.Settings.Default.InjectDecal;
            AccountName = Properties.Settings.Default.SimpleLaunch_Username;
            Password = Properties.Settings.Default.SimpleLaunch_Password;
            var initialServer = _servers.SourceCollection.OfType<Server.ServerItem>().FirstOrDefault(x => x.ServerName == Properties.Settings.Default.SimpleLaunch_ServerName);
            SelectedServer = initialServer;
        }
        public void SaveToSettings()
        {
            Properties.Settings.Default.InjectDecal = UseDecal;
            Properties.Settings.Default.SimpleLaunch_Username = AccountName;
            Properties.Settings.Default.SimpleLaunch_Password = Password;
            Properties.Settings.Default.SimpleLaunch_ServerName = (SelectedServer != null ? SelectedServer.ServerName : "");
            Properties.Settings.Default.Save();
        }
        private readonly CollectionView _servers;
        public CollectionView Servers { get { return _servers; } }
        public Server.ServerItem SelectedServer { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public bool UseDecal { get; set; }
        public bool UseDecalEnabled { get { return DecalInjection.IsDecalInstalled(); } }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void PerformSimpleLaunch()
        {
            string path = Properties.Settings.Default.ACLocation; // "c:\\Turbine\\Asheron's Call\\acclient.exe";
            LaunchSimpleGame(path, SelectedServer, AccountName, Password);
        }
        private void LaunchSimpleGame(string path, Server.ServerItem server, string account, string pwd)
        {
            SaveToSettings();
            var launchItem = new LaunchItem();
            launchItem.CustomLaunchPath = path;
            launchItem.ServerName = server.ServerName;
            launchItem.AccountName = account;
            launchItem.Password = pwd;
            launchItem.ipAddress = server.ServerIP;
            launchItem.EMU = server.EMU;
            launchItem.CharacterSelected = null; // no character choices for SimpleLaunch, b/c that requires MagFilter
            launchItem.RodatSetting = server.RodatSetting;
            launchItem.IsSimpleLaunch = true;

            if (LaunchingEvent == null) { throw new Exception("SimpleLaunchWindowViewModel.LaunchingEvent null"); }
            LaunchingEvent(launchItem);

            ////var launcher = new GameLauncher();
            ////GameLaunchResult glr = launcher.LaunchGameClient(path, server.ServerName, account, pwd, server.ServerIP, server.EMU, null, server.RodatSetting);
            ////return glr;
        }
    }
}
