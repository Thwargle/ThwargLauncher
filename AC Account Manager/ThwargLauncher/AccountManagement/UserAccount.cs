using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ThwarglePropertyExtensions;

namespace ThwargLauncher
{
    public class UserAccount : INotifyPropertyChanged
    {
        public UserAccount(string accountName)
        {
            this.Name = accountName;
        }
        public UserAccount(string name, string password)
        {
            this.Name = name;
            this.Password = password;
        }
        public bool IsServerEnabled(string serverName)
        {
            string propName = serverName + "Enabled";
            string value = GetPropertyByName(propName);
            if (string.IsNullOrEmpty(value)) { return false; }
            return bool.Parse(value);
        }
        public void SetServerEnabled(string serverName, bool enabled)
        {
            string propName = serverName + "Enabled";
            SetPropertyByName(propName, enabled.ToString());
        }
        private void InitializeMe(MagFilter.CharacterBook characterBook)
        {
            foreach (var serverItem in ServerManager.ServerList)
            {
                //TODO: Actual Server Selection
                //if (!IsServerEnabled(serverName)) { continue; }
                // Get characters from dll
                MagFilter.ServerCharacterListByAccount charlist = null;
                if (characterBook != null)
                {
                    charlist = characterBook.GetCharacters(serverName: serverItem.ServerName, accountName: this.Name);   
                }
                // Construct server & character data
                var server = new Server(serverItem);

                server.ChosenCharacter = "None";

                if (charlist != null)
                {
                    LoadCharacterListFromMagFilterData(server, charlist.CharacterList);
                    this.ZoneId = charlist.ZoneId; // recording this each time through this loop, but it will be the same so that is okay
                }
                server.PropertyChanged += ServerPropertyChanged;
                // Record data
                _servers.Add(server);
            }
        }
        public void LoadCharacterListFromMagFilterData(Server server, List<MagFilter.Character> magchars)
        {
            server.AvailableCharacters.Clear();
            //create and add a default character of none.
            var defaultChar = new AccountCharacter()
            {
                Id = 0,
                Name = "None"
            };
            server.AvailableCharacters.Add(defaultChar);
            if (magchars != null)
            {
                foreach (var dllChar in magchars)
                {
                    var acctChar = new AccountCharacter()
                    {
                        Id = 99, // TODO - not used
                        Name = dllChar.Name
                    };
                    server.AvailableCharacters.Add(acctChar);
                }
            }
        }
        /// <summary>
        /// Used to load data from file on disk
        /// </summary>
        public void LoadAllProperties(MagFilter.CharacterBook characterBook, Dictionary<string, string> properties)
        {
            foreach (KeyValuePair<string, string> property in properties)
            {
                _properties[property.Key] = property.Value;
            }
            InitializeMe(characterBook);
        }

        public void NotifyAccountSummaryChanged()
        {
            OnPropertyChanged("AccountSummary");
        }
        public void NotifyVisibleServersChanged()
        {
            OnPropertyChanged("VisibleServers");
        }
        public void NotifyAvailableCharactersChanged()
        {
            OnPropertyChanged("AvailableCharacters");
        }

        void ServerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ServerSelected" || e.PropertyName == "ChosenCharacter")
            {
                OnPropertyChanged("AccountSummary");
            }
        }

        //private string _name = "Unspecified";
        private readonly List<Server> _servers = new List<Server>();
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public List<Server> Servers
        {
            get { return _servers; }
        }
        public List<Server> ActivatedServers
        {
            get { return _servers.Where(x => x.ServerSelected).ToList(); }
        }
        public List<Server> VisibleServers
        {
            get { return _servers.Where(x => x.VisibilitySetting == ServerModel.VisibilityEnum.Visible).ToList(); }
        }

        public string AccountSummary
        {
            get
            {
                var serverInfos = new List<string>();
                foreach (Server server in _servers)
                {
                    if (server.ServerSelected)
                    {
                        if (ServerHasChosenCharacter(server))
                        {
                            string entry = string.Format("{0}{1}->{2}", server.ServerStatusSymbol, server.ServerName, server.ChosenCharacter);
                            // architectural problem getting to game session here
                            GameSession session = AppCoordinator.GetTheGameSessionByServerAccount(serverName: server.ServerName, accountName: this.Name);
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
                            serverInfos.Add(entry);
                        }
                        else
                        {
                            string entry = server.ServerName;
                            serverInfos.Add(entry);
                        }
                    }
                }
                string text = DisplayName;
                if (serverInfos.Any())
                {
                    text += ": " + string.Join(", ", serverInfos);
                }
                return text;
            }
        }
        private string SummarizeUptime(GameSession session)
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
        private bool ServerHasChosenCharacter(Server server)
        {
            if (string.IsNullOrEmpty(server.ChosenCharacter)) { return false; }
            if (server.ChosenCharacter == "None") { return false; }
            return true;
        }
        public string GetPropertyByName(string key) { return GetPropertyValue(key); }
        private string GetPropertyValue(string key)
        {
            if (_properties.ContainsKey(key))
            {
                return _properties[key];
            }
            else
            {
                return null;
            }
        }
        public void SetPropertyByName(string key, string value) { SetPropertyValue(key, value); }
        private void SetPropertyValue(string key, string value)
        {
            if (!_properties.ContainsKey(key) || _properties[key] != value)
            {
                _properties[key] = value;
                //OnPropertyChanged(key);
            }
        }
        public IDictionary<string, string> GetAllProperties() { return _properties; }

        public string Name { get { return GetPropertyValue("Name"); } set { SetPropertyValue("Name", value); } }
        public string Password { get { return GetPropertyValue("Password"); } set { SetPropertyValue("Password", value); } }
        public string CustomLaunchPath { get { return GetPropertyValue("LaunchPath"); } }
        public string CustomPreferencePath { get { return GetPropertyValue("PreferencePath"); } }
        public string Alias { get { return GetPropertyValue("Alias"); } set { SetPropertyValue("Alias", value); } }
        public string Priority { get { return GetPropertyValue("Priority"); } set { SetPropertyValue("Priority", value); } }
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Alias))
                {
                    return Alias;
                }
                else
                {
                    return Name;
                }
            }
        }

        public bool AccountLaunchable { get; set; }
        public string ZoneId { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Raise(this, propertyName);
        }
    }
}