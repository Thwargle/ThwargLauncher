using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThwargLauncher
{
    public class Server : INotifyPropertyChanged
    {
        public Server(string serverName)
        {
            AvailableCharacters = new List<AccountCharacter>();
            ServerStatusSymbol = "";
            ServerName = serverName;
            ServerIP = "127.0.0.1:9000";
        }

        public string ServerStatusSymbol { get; set; }
        public string ServerIP { get; set; }
        public string ServerName { get; set; }
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

    static class ServerManager
    {
        public static List<string> ServerList = new List<string>()
            {
                "Local",
                "Frostfell",
                "Thistledown",
                "Harvestgain",
                "Verdantine",
                "Leafcull",
                "WintersEbb",
                "Morningthaw",
                "Darktide",
                "Solclaim"
            };
    }

}
