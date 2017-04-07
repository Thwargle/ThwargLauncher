using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;
using CommonControls;

namespace ThwargLauncher
{
    public class SimpleLaunchWindowViewModel : INotifyPropertyChanged
    {
        public event System.EventHandler RequestingMainViewEvent;
        public event LaunchGameDelegateMethod LaunchingEvent;
        public ICommand GotoMainViewCommand { get; private set; }
        public Action CloseAction { get; set; }

        public static SimpleLaunchWindowViewModel CreateViewModel()
        {
            var vmodel = new SimpleLaunchWindowViewModel();
            return vmodel;
        }
        private SimpleLaunchWindowViewModel()
        {
            IEnumerable<SimpleServerItem> items = ServerManager.ServerList.Select(p => new SimpleServerItem(p));
            //IEnumerable<ServerInfo> items = ServerManager.ServerList;
            _servers = new CollectionView(items);
            GotoMainViewCommand = new DelegateCommand(
                    PerformGotoMainView
                );

            LoadFromSettings();
        }
        public void LoadFromSettings()
        {
            UseDecal = Properties.Settings.Default.InjectDecal;
            AccountName = Properties.Settings.Default.SimpleLaunch_Username;
            Password = Properties.Settings.Default.SimpleLaunch_Password;
            var initialServer = _servers.SourceCollection.OfType<SimpleServerItem>().FirstOrDefault(
                x => x.GetHashCode() == Properties.Settings.Default.SimpleLaunch_ServerHashCode);
            SelectedServer = initialServer;
        }
        public void SaveToSettings()
        {
            Properties.Settings.Default.InjectDecal = UseDecal;
            Properties.Settings.Default.SimpleLaunch_Username = AccountName;
            Properties.Settings.Default.SimpleLaunch_Password = Password;
            Properties.Settings.Default.SimpleLaunch_ServerHashCode = (SelectedServer != null ? SelectedServer.GetHashCode() : 0);
            Properties.Settings.Default.Save();
        }
        private readonly CollectionView _servers;
        public CollectionView Servers { get { return _servers; } }
        public SimpleServerItem SelectedServer { get; set; }
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
            LaunchSimpleGame(path, SelectedServer.ServerItem, AccountName, Password);
        }
        private void LaunchSimpleGame(string path, ServerInfo server, string account, string pwd)
        {
            SaveToSettings();
            var launchItem = new LaunchItem();
            launchItem.CustomLaunchPath = path;
            launchItem.ServerName = server.ServerName;
            launchItem.AccountName = account;
            launchItem.Password = pwd;
            launchItem.IpAndPort = server.ServerIpAndPort;
            launchItem.EMU = server.EMU;
            launchItem.CharacterSelected = null; // no character choices for SimpleLaunch, b/c that requires MagFilter
            launchItem.RodatSetting = server.RodatSetting;
            launchItem.IsSimpleLaunch = true;

            if (LaunchingEvent == null) { throw new Exception("SimpleLaunchWindowViewModel.LaunchingEvent null"); }
            LaunchingEvent(launchItem);
        }
        private void PerformGotoMainView()
        {
            if (RequestingMainViewEvent != null)
            {
                RequestingMainViewEvent(this, new EventArgs());
            }
        }
    }
}
