using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AC_Account_Manager
{
    public class UserAccount : INotifyPropertyChanged
    {
        public UserAccount(string accountName, MagFilter.CharacterManager characterMgr)
        {
            this.Name = accountName;
            InitializeMe(characterMgr);
        }
        public UserAccount(string name, string password)
        {
            this.Name = name;
            this.Password = password;
            InitializeMe(null);
        }
        private void InitializeMe(MagFilter.CharacterManager characterMgr)
        {
            foreach (var serverName in ServerManager.ServerList)
            {
                // Get characters from dll
                MagFilter.ServerCharacterListByAccount charlist = null;
                if (characterMgr != null)
                {
                    charlist = characterMgr.GetCharacters(serverName: serverName, accountName: this.Name);
                    
                }
                // Construct server & character data
                var server = new Server(serverName);
                //create and add a default character of none.
                var defaultChar = new AccountCharacter()
                {
                    Id = 0,
                    Name = "None"
                };
                server.AvailableCharacters.Add(defaultChar);
                server.ChosenCharacter = "None";

                if (charlist != null)
                {
                    foreach (var dllChar in charlist.CharacterList)
                    {
                        var acctChar = new AccountCharacter()
                        {
                            Id = 99, // TODO - not used
                            Name = dllChar.Name
                        };
                        server.AvailableCharacters.Add(acctChar);
                    }
                    this.ZoneId = charlist.ZoneId; // recording this each time through this loop, but it will be the same so that is okay
                }
                server.PropertyChanged += server_PropertyChanged;
                // Record data
                _servers.Add(server);
            }
        }
        /// <summary>
        /// Used to load data from file on disk
        /// </summary>
        public void LoadAllProperties(Dictionary<string, string> properties)
        {
            foreach (KeyValuePair<string, string> property in properties)
            {
                _properties[property.Key] = property.Value;
            }
        }

        void server_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ServerSelected")
            {
                OnPropertyChanged("AccountSummary");
            }
        }

        private string _name = "Unspecified";
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

        public string AccountSummary
        {
            get
            {
                string displayName = DisplayName;
                string serverList = string.Join(", ", ActivatedServers);
                return string.Format("{0} - {1}", displayName, serverList);
            }
        }
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
        private void SetPropertyValue(string key, string value)
        {
            if (!_properties.ContainsKey(key) || _properties[key] != value)
            {
                _properties[key] = value;
                // Do we need to notify property change?
            }
        }
        public IDictionary<string, string> GetAllProperties() { return _properties; }

        public string Name { get { return GetPropertyValue("Name"); } private set { SetPropertyValue("Name", value); } }
        public string Password { get { return GetPropertyValue("Password"); } set { SetPropertyValue("Password", value); } }
        public string CustomLaunchPath { get { return GetPropertyValue("LaunchPath"); } }
        public string CustomPreferencePath { get { return GetPropertyValue("PreferencePath"); } }
        public string Alias { get { return GetPropertyValue("Alias"); } set { SetPropertyValue("Alias", value); } }
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}