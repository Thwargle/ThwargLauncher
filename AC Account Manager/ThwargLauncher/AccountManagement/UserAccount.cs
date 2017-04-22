using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                var server = new Server(this, serverItem);

                server.ChosenCharacter = "None";

                LoadCharacterListFromMagFilterData(server, charlist);
                if (charlist != null)
                {
                    this.ZoneId = charlist.ZoneId; // recording this each time through this loop, but it will be the same so that is okay
                }
                server.PropertyChanged += ServerPropertyChanged;
                // Record data
                _servers.Add(server);
            }
        }
        public void LoadCharacterListFromMagFilterData(Server server, MagFilter.ServerCharacterListByAccount charlist)
        {
            server.AvailableCharacters.Clear();
            //create and add a default character of none.
            var defaultChar = new AccountCharacter()
            {
                Id = 0,
                Name = "None"
            };
            server.AvailableCharacters.Add(defaultChar);

            if (charlist != null)
            {
                List<MagFilter.Character> magchars = charlist.CharacterList;
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
            if (e.PropertyName == "ServerSelected")
            {
                OnPropertyChanged("SelectedServers");
            }
            if (e.PropertyName == "VisibilitySetting")
            {
                OnPropertyChanged("VisibleServers");
            }
        }

        //private string _name = "Unspecified";
        private readonly ObservableCollection<Server> _servers = new ObservableCollection<Server>();
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public ObservableCollection<Server> Servers
        {
            get { return _servers; }
        }
        public List<Server> ActivatedServers
        {
            get { return _servers.Where(x => x.ServerSelected).ToList(); }
        }
        public ObservableCollection<Server> VisibleServers
        {
            get { return new ObservableCollection<Server>(_servers.Where(x => x.VisibilitySetting == ServerModel.VisibilityEnum.Visible)); }
        }
        public ObservableCollection<Server> SelectedServers
        {
            get { return new ObservableCollection<Server>(_servers.Where(x => x.ServerSelected)); }
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
                        string entry = server.StatusSummary;
                        serverInfos.Add(entry);
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