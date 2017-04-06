using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
            private string _connectionStatus = "?";
            private System.Windows.Media.SolidColorBrush _connectionColor = System.Windows.Media.Brushes.AntiqueWhite;
            private ServerUpStatus _upStatus;
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
            public System.Windows.Media.SolidColorBrush ConnectionColor
            {
                get { return _connectionColor; }
                set
                {
                    if (_connectionColor != value)
                    {
                        _connectionColor = value;
                        OnPropertyChanged("ConnectionColor");
                    }
                }
            }
            public enum ServerUpStatus {  Unknown, Down, Up };
            public ServerUpStatus UpStatus
            {
                get { return _upStatus; }
                set
                {
                    if (_upStatus != value)
                    {
                        _upStatus = value;
                        ConnectionColor = GetBrushColorFromUpStatus(_upStatus);
                        OnPropertyChanged("UpStatus");
                    }
                }
            }
            private System.Windows.Media.SolidColorBrush GetBrushColorFromUpStatus(ServerUpStatus upStatus)
            {
                switch (_upStatus)
                {
                    case ServerUpStatus.Down: return System.Windows.Media.Brushes.Red;
                    case ServerUpStatus.Up: return System.Windows.Media.Brushes.Lime;
                    default: return System.Windows.Media.Brushes.AntiqueWhite;
                }
            }
        }
        private string _serverStatusSymbol;
        public string ServerStatusSymbol
        {
            get
            {
                return _serverStatusSymbol;
            }
            set
            {
                if (_serverStatusSymbol != value)
                {
                    _serverStatusSymbol = value;
                    OnPropertyChanged("ServerStatusSymbol");
                }
            }
        }
        public string ServerIpAndPort { get { return _myServerItem.ServerIpAndPort; } }
        public string ServerName { get { return _myServerItem.ServerName; } }
        public string EMU { get {  return _myServerItem.EMU; } }
        public string RodatSetting { get { return _myServerItem.RodatSetting; } }
        public string ConnectionStatus { get { return _myServerItem.ConnectionStatus; } }
        public System.Windows.Media.SolidColorBrush ConnectionColor {  get { return _myServerItem.ConnectionColor;  } }
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
}
