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
            ServerName = serverName;
        }

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
        public string ChosenCharacter { get; set; }
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
                "Frostfell",
                "Thistledown",
                "Harvestgain",
                "Verdantine",
                "Leafcull",
                "Wintersebb",
                "Morningthaw",
                "Darktide",
                "Solclaim"
            };
    }

}
