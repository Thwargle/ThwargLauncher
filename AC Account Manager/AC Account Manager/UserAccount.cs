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
            foreach (var serverName in ServerManager.ServerList)
            {
                // Get characters from dll
                var charlist = characterMgr.GetCharacters(serverName: serverName, accountName: accountName);
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
                }
                if (charlist != null)
                {
                    this.ZoneId = charlist.ZoneId; // recording this each time through this loop, but it will be the same so that is okay
                }
                server.PropertyChanged += server_PropertyChanged;
                // Record data
                _servers.Add(server);
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
                return string.Format(
                    "{0} - {1}",
                    Name,
                    string.Join(", ", ActivatedServers)
                    );
            }
        }
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        public string Password { get; set; }

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