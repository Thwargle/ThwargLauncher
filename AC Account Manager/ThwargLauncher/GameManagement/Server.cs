using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThwargLauncher
{
    public class Server : INotifyPropertyChanged
    {
        public Server(string serverName, string serverIP, string emu)
        {
            AvailableCharacters = new List<AccountCharacter>();
            ServerStatusSymbol = "";
            ServerName = serverName;
            ServerIP = serverIP;
            EMU = emu;
        }

        public class ServerItem
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }
            public string EMU { get; set; }
        }

        public string ServerStatusSymbol { get; set; }
        public string ServerIP { get; set; }
        public string ServerName { get; set; }
        public string EMU { get; set; }
        public string ServerDisplayName { get { return string.Format("{0} - {1}", EMU, ServerName); } }
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
