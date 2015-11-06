using System.Collections.Generic;

namespace AC_Account_Manager
{
    public class Server
    {
        public Server(string serverName)
        {
            AvailableCharacters = new List<AccountCharacter>();
            ServerName = serverName;
        }

        public string ServerName { get; set; }
        public bool ServerSelected { get; set; }
        public List<AccountCharacter> AvailableCharacters { get; private set; }
        public string ChosenCharacter { get; set; }
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
