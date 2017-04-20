using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ThwarglePropertyExtensions;

namespace ThwargLauncher
{
    /// <summary>
    /// This holds the data for one Account+Server combination
    /// which is a list of characters and the character selected, and this server's status (in the context of this account)
    /// The server properties are all forwarded from a pointer to the ServerModel of the relevant server
    /// </summary>
    public class Server : INotifyPropertyChanged
    {
        public Server(UserAccount acct, ServerModel serverItem)
        {
            _myAccount = acct;
            _myServer = serverItem;
            _myServer.PropertyChanged += ServerItemPropertyChanged;
            AvailableCharacters = new ObservableCollection<AccountCharacter>();
            ServerStatusSymbol = "";
        }

        void ServerItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ServerName" || e.PropertyName == "ServerDescription")
            {
                OnPropertyChanged("ServerDisplayName");
            }
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
                    OnPropertyChanged("StatusSummary");
                }
            }
        }
        public string ServerIpAndPort { get { return _myServer.ServerIpAndPort; } }
        public System.Guid ServerId { get { return _myServer.ServerId; } }
        public string ServerName { get { return _myServer.ServerName; } }
        public ServerModel.ServerEmuEnum EMU { get {  return _myServer.EMU; } }
        public ServerModel.RodatEnum RodatSetting { get { return _myServer.RodatSetting; } }
        public ServerModel.VisibilityEnum VisibilitySetting { get { return _myServer.VisibilitySetting; } }
        public string ConnectionStatus { get { return _myServer.ConnectionStatus; } }
        public System.Windows.Media.SolidColorBrush ConnectionColor {  get { return _myServer.ConnectionColor;  } }
        public string IsPublished {  get { return _myServer.ServerSource == ServerModel.ServerSourceEnum.Published ? "True" : "False"; } }
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
        private readonly UserAccount _myAccount;
        private readonly ServerModel _myServer;
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
        public ObservableCollection<AccountCharacter> AvailableCharacters { get; private set; }
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
        public string StatusSummary
        {
            get
            {
                string entry = ServerName;
                if (HasChosenCharacter)
                {
                    entry = string.Format("{0}{1}", ServerStatusSymbol, ServerName);
                    entry += string.Format("->{0}", ChosenCharacter);
                    // architectural problem getting to game session here
                    GameSession session = AppCoordinator.GetTheGameSessionByServerAccount(serverName: ServerName, accountName: _myAccount.Name);
                    if (session != null)
                    {
                        if (session.UptimeSeconds > 0)
                        {
                            entry += " [" + SummarizeUptime(session) + "]";
                        }
                        if (session.TeamCount > 0)
                        {
                            entry += " (" + session.TeamList + ")";
                        }
                    }
                }
                return entry;
            }
        }
        private bool HasChosenCharacter
        {
            get
            {
                if (string.IsNullOrEmpty(ChosenCharacter)) { return false; }
                if (ChosenCharacter == "None") { return false; }
                return true;
            }
        }
        private static string SummarizeUptime(GameSession session)
        {
            if (session.UptimeSeconds < 60)
            {
                return string.Format("{0}s", session.UptimeSeconds);
            }
            if (session.UptimeSeconds < 60 * 60)
            {
                return string.Format("{0}m", session.UptimeSeconds / 60);
            }
            if (session.UptimeSeconds < 60 * 60 * 24)
            {
                return string.Format("{0}h", session.UptimeSeconds / (60 * 60));
            }
            return string.Format("{0}d", session.UptimeSeconds / (60 * 60 * 24));
        }
        public override string ToString()
        {
            return ServerName;
        }

        public void NotifyAvailableCharactersChanged()
        {
            OnPropertyChanged("AvailableCharacters");
            // Do wo need to also send one for ChosenCharacter?
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Raise(this, propertyName);
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
