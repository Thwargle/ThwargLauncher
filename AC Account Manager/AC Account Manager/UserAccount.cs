using System.Collections.Generic;

namespace AC_Account_Manager
{
    public class UserAccount
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
                // Record data
                _servers.Add(server);
            }
        }

        private string _name = "Unspecified";
        private readonly List<Server> _servers = new List<Server>();

        public List<Server> Servers
        {
            get { return _servers; }
        }


        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        public string Password { get; set; }

        public bool AccountLaunchable { get; set; }
        public string ZoneId { get; private set; }
    }
}