using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;


namespace ThwargLauncher
{
    public class SimpleLaunchWindowViewModel : INotifyPropertyChanged
    {
        public static SimpleLaunchWindowViewModel CreateViewModel()
        {
            var vmodel = new SimpleLaunchWindowViewModel();
            return vmodel;
        }
        private SimpleLaunchWindowViewModel()
        {
            _servers = new CollectionView(ServerManager.ServerList);
            if (_servers.Count > 10) // Should be 0, but testing the UI validation
            {
                SelectedServer = (Server.ServerItem)_servers.GetItemAt(0);
            }
        }
        private readonly CollectionView _servers;
        public CollectionView Servers { get { return _servers; } }
        public Server.ServerItem SelectedServer { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
