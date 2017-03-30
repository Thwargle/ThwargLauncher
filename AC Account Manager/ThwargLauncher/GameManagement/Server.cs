using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThwargLauncher
{
    public class Server : INotifyPropertyChanged
    {
        public Server(ServerItem serverItem)
        {
            _myServerItem = serverItem;
            AvailableCharacters = new List<AccountCharacter>();
            ServerStatusSymbol = "";
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
            public string ServerIP { get; set; }
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
        public string ServerIP { get { return _myServerItem.ServerIP; } }
        public string ServerName { get { return _myServerItem.ServerName; } }
        public string EMU { get {  return _myServerItem.EMU; } }
        public string RodatSetting { get { return _myServerItem.RodatSetting; } }
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

    public static class ServerManager
    {
        public static List<Server.ServerItem> ServerList = new List<Server.ServerItem>();
    }

}
