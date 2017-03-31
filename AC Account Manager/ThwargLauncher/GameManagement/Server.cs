using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ThwargLauncher.UtilityCode;

namespace ThwargLauncher
{
    public class Server : INotifyPropertyChanged
    {
        public Server(ServerItem serverItem)
        {
            _myServerItem = serverItem;
            _myServerItem.PropertyChanged += ServerItemPropertyChanged;
            AvailableCharacters = new List<AccountCharacter>();
            ServerStatusSymbol = "";
        }

        void ServerItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public class ServerItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }

            public string ServerName { get; set; }
            public string ServerDescription { get; set; }
            public bool ServerLoginEnabled { get; set; }
            public string ServerIpAndPort { get; set; }
            public string EMU { get; set; }
            public string RodatSetting { get; set; }
            private string _connectionStatus;
            public string ConnectionStatus
            {
                get { return _connectionStatus; }
                set
                {
                    if (_connectionStatus != value)
                    {
                        _connectionStatus = value;
                        OnPropertyChanged("ConnectionStatus");
                    }
                }
            }
        }
        public string ServerStatusSymbol { get; set; }
        public string ServerIpAndPort { get { return _myServerItem.ServerIpAndPort; } }
        public string ServerName { get { return _myServerItem.ServerName; } }
        public string EMU { get {  return _myServerItem.EMU; } }
        public string RodatSetting { get { return _myServerItem.RodatSetting; } }
        public string ConnectionStatus { get { return _myServerItem.ConnectionStatus; } }
        public string ServerDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_myServerItem.ServerDescription))
                {
                    return string.Format("{0}", ServerName);
                }
                else
                {
                    string desc = _myServerItem.ServerDescription;
                    const int MAXLEN = 64;
                    if (desc.Length > MAXLEN)
                    {
                        desc = desc.Substring(0, MAXLEN - 3) + "...";
                    }
                    return string.Format("{0} - {1}", ServerName, desc);
                }
            }
        }
        readonly private ServerItem _myServerItem;
        private bool _serverSelected;
        public bool ServerSelected
        {
            get { return _serverSelected; }
            set
            {
                if (_serverSelected != value)
                {
                    _serverSelected = value;
                    OnPropertyChanged("ServerSelected");
                }
            }
        }
        public List<AccountCharacter> AvailableCharacters { get; private set; }

        private string _chosenCharacter;
        public string ChosenCharacter
        {
            get { return _chosenCharacter; }
            set
            {
                if (_chosenCharacter != value)
                {
                    _chosenCharacter = value;
                    OnPropertyChanged("ChosenCharacter");
                }
            }
        }
        
        public override string ToString()
        {
            return ServerName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class AddressParser
    {
        public class Address
        {
            public string Ip { get; set; }
            public int Port { get; set; }
        }
        public static Address Parse(string text)
        {
            var address = new Address();
            int index = text.IndexOf(':');
            if (index > 0)
            {
                address.Ip = text.Substring(0, index);
                if (index < text.Length - 1)
                {
                    int val = 0;
                    if (int.TryParse(text.Substring(index + 1), out val))
                    {
                        address.Port = val;
                    }
                }
            }
            return address;
        }
    }
    public class ServerManager
    {
        public static List<Server.ServerItem> ServerList = new List<Server.ServerItem>();
        public static bool IsLoaded;

        public void LoadServerLists()
        {
            var phatServers = (new GameManagement.PhatACServerLister()).loadPhatServers();
            var aceServers = (new GameManagement.AceServerLister()).loadACEServers();
            foreach (var serverItem in phatServers.DistinctBy(p => p.GetHashCode()))
            {
                AddOrUpdateServer(serverItem);
            }
            foreach (var serverItem in aceServers.DistinctBy(p => p.GetHashCode()))
            {
                AddOrUpdateServer(serverItem);
            }
            IsLoaded = true;
        }
        private void AddOrUpdateServer(Server.ServerItem server)
        {
            var existing = ServerList.FirstOrDefault(s => s.GetHashCode() == server.GetHashCode());
            if (existing != null)
            {
                // Currently we don't update because our GUI doesn't support editing existing servers
            }
            else
            {
                ServerList.Add(server);
            }
        }
    }


}
