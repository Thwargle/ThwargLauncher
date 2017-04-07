using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ThwargLauncher
{
    public class Server : INotifyPropertyChanged
    {
        public Server(ServerInfo serverItem)
        {
            _myServer = serverItem;
            _myServer.PropertyChanged += ServerItemPropertyChanged;
            AvailableCharacters = new List<AccountCharacter>();
            ServerStatusSymbol = "";
        }

        void ServerItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
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
        public string ServerIpAndPort { get { return _myServer.ServerIpAndPort; } }
        public string ServerName { get { return _myServer.ServerName; } }
        public string EMU { get {  return _myServer.EMU; } }
        public string RodatSetting { get { return _myServer.RodatSetting; } }
        public string ConnectionStatus { get { return _myServer.ConnectionStatus; } }
        public System.Windows.Media.SolidColorBrush ConnectionColor {  get { return _myServer.ConnectionColor;  } }
        public string ServerDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_myServer.ServerDescription))
                {
                    return string.Format("{0}", ServerName);
                }
                else
                {
                    string desc = _myServer.ServerDescription;
                    const int MAXLEN = 64;
                    if (desc.Length > MAXLEN)
                    {
                        desc = desc.Substring(0, MAXLEN - 3) + "...";
                    }
                    return string.Format("{0} - {1}", ServerName, desc);
                }
            }
        }
        readonly private ServerInfo _myServer;
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
