using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;
using CommonControls;
using System.Windows;

namespace ThwargLauncher
{
    public class SimpleLaunchWindowViewModel : INotifyPropertyChanged
    {
        public event EventHandler RequestingMainViewEvent;
        public event EventHandler RequestingConfigureFileLocationEvent;
        public event LaunchGameDelegateMethod LaunchingEvent;
        public ICommand GotoMainViewCommand { get; private set; }
        public ICommand ConfigureFileLocationCommand { get; private set; }
        public Action CloseAction { get; set; }
        public string ClientFileLocation
        {
            get
            {
                return TryGetClientFileLocation();
            }
            set
            {
                if (Properties.Settings.Default.ACLocation != value)
                {
                    Properties.Settings.Default.ACLocation = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged("ClientFileLocation");
                }
            }
        }
        private string TryGetClientFileLocation()
        {
            try
            {
                return Properties.Settings.Default.ACLocation;
            }
            catch
            {
                return @"C:\Turbine\Asheron's Call\acclient.exe";
            }
        }

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
            ConfigureFileLocationCommand = new DelegateCommand(
                    PerformConfigureFileLocation
                );

            

            LoadFromSettings();
        }
        public void LoadFromSettings()
        {
            SimpleServerItem initialServer = null;
            try
            {
                UseDecal = Properties.Settings.Default.InjectDecal;
                AccountName = Properties.Settings.Default.SimpleLaunch_Username;
                Password = Properties.Settings.Default.SimpleLaunch_Password;
                initialServer = _servers.SourceCollection.OfType<SimpleServerItem>().FirstOrDefault(
                    x => x.GetHashCode() == Properties.Settings.Default.SimpleLaunch_ServerHashCode);
            }
            catch
            {
            }
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
        private void LaunchSimpleGame(string path, ServerModel server, string account, string pwd)
        {
            SaveToSettings();
            var launchItem = new LaunchItem();
            launchItem.CustomLaunchPath = path;
            launchItem.ServerName = server.ServerName;
            launchItem.AccountName = account;
            launchItem.Password = pwd;
            launchItem.IpAndPort = server.ServerIpAndPort;
            launchItem.EMU = server.EMU;
            launchItem.CharacterSelected = null; // no character choices for SimpleLaunch, b/c that requires ThwargFilter
            launchItem.RodatSetting = server.RodatSetting;
            launchItem.SecureSetting = server.SecureSetting;
            launchItem.IsSimpleLaunch = true;

            if (LaunchingEvent == null) { throw new Exception("SimpleLaunchWindowViewModel.LaunchingEvent null"); }
            LaunchingEvent(launchItem);
        }
        private void PerformGotoMainView()
        {
            if (RequestingMainViewEvent != null)
            {
                if (!DecalInjection.IsDecalInstalled() || !DecalInjection.IsThwargFilterRegistered())
                {
                    StringBuilder warning = new StringBuilder();
                    if (!DecalInjection.IsDecalInstalled())
                    {
                        warning.Append("Decal is not installed properly. ");
                    }
                    if (!DecalInjection.IsThwargFilterRegistered())
                    {
                        warning.Append("ThwargFilter is not registered properly. ");
                    }
                    //Does not seem to work yet; investigate how decal saves enabled
                    //if (!DecalInjection.IsThwargFilterEnabled())
                    //{
                    //    warning.Append("ThwargFilter appears to be disabled. ");
                    //}
                    warning.Append("This may cause issues (such as clients being continually restarted) in advanced mode. Are you sure you want to continue to advanced mode?");
                    if (MessageBox.Show(warning.ToString(), "Configuration issue", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                RequestingMainViewEvent(this, new EventArgs());
            }
        }
        private void PerformConfigureFileLocation()
        {
            if (RequestingConfigureFileLocationEvent != null)
            {
                RequestingConfigureFileLocationEvent(this, new EventArgs());
                OnPropertyChanged("ClientFileLocation");
            }
        }
    }
}
