using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ThwargFilter
{
    public class CharacterBook
    {
        private Dictionary<string, ServerCharacterListByAccount> _data = null;

        /*
        private CharacterBook(Dictionary<string, ServerCharacterListByAccount> dictionary)
        {
            _data = dictionary;
        }
        */
        private CharacterBook()
        {
            _data = new Dictionary<string, ServerCharacterListByAccount>();
        }
        private void AppendCharacters(Dictionary<string, ServerCharacterListByAccount> dictionary)
        {
            foreach (KeyValuePair<string, ServerCharacterListByAccount> pair in dictionary)
            {
                string key = pair.Key;
                ServerCharacterListByAccount charlist = pair.Value;
                if (this._data.ContainsKey(key))
                {
                    throw new Exception("Shouldn't have two character files for same key: " + key);
                }
                else
                {
                    this._data.Add(key, charlist);
                }
            }

        }
        public IEnumerable<string> GetKeys()
        {
            return _data.Keys;
        }
        public ServerCharacterListByAccount GetCharacters(string serverName, string accountName)
        {
            string key = GetKey(server: serverName, accountName: accountName);
            if (this._data.ContainsKey(key))
            {
                return this._data[key];
            }
            else
            {
                return null;
            }
        }
        public ServerCharacterListByAccount GetCharactersOrEmpty(string serverName, string accountName)
        {
            var list = GetCharacters(serverName, accountName: accountName);
            if (list != null)
            {
                return list;
            }
            else
            {
                return new ServerCharacterListByAccount();
            }
        }
        internal ServerCharacterListByAccount GetCharacters(string key)
        {
            return this._data[key];
        }
        private static string GetKey(string server, string accountName)
        {
            string key = string.Format("{0}-{1}", server, accountName);
            return key;
        }

        public static void WriteCharacters(string ServerName, string zonename, List<Character> characters)
        {
            var launchInfo = LaunchControl.GetLaunchInfo();
            if (!launchInfo.IsValid)
            {
                log.WriteError("LaunchInfo not valid");
                return;
            }

            if (!IsValidCharacterName(launchInfo.CharacterName))
            {
                try
                {
                    log.WriteInfo("WriteCharacters called with no character name, so writing launch response");
                    LaunchControl.RecordLaunchResponse(DateTime.UtcNow);
                }
                catch
                {
                    log.WriteError("WriteCharacters: Exception trying to record launch response");
                }
            }

            // Pass info to Heartbeat
            Heartbeat.RecordServer(launchInfo.ServerName);
            Heartbeat.RecordAccount(launchInfo.AccountName);

            string key = GetKey(server: launchInfo.ServerName, accountName: launchInfo.AccountName);
            var clist = new ServerCharacterListByAccount()
            {
                ZoneId = zonename,
                CharacterList = characters
            };

            // Create a dictionary of only our characters to save
            Dictionary<string, ServerCharacterListByAccount> solodict = new Dictionary<string, ServerCharacterListByAccount>();
            solodict[key] = clist;

            string contents = JsonConvert.SerializeObject(solodict, Formatting.Indented);
            string path = FileLocations.GetCharacterFilePath(ServerName: launchInfo.ServerName, AccountName: launchInfo.AccountName);
            using (var file = new StreamWriter(path, append: false))
            {
                file.Write(contents);
            }
        }

        private static bool IsValidCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) { return false; }
            if (characterName == "None") { return false; }
            return true;
        }

        public static CharacterBook ReadCharacters()
        {
            try
            {
                return ReadCharactersImpl();
            }
            catch (Exception exc)
            {
                log.WriteError("ReadCharacterImpl Exception: " + exc.ToString());
                return null;
            }
        }

        private static CharacterBook ReadCharactersImpl()
        {
            CharacterBook charMgr = new CharacterBook();
            foreach (string filepath in EnumerateCharacterFilepaths())
            {
                using (var file = new StreamReader(filepath))
                {
                    string contents = file.ReadToEnd();
                    // to avoid json vulnerability, do not use TypeNameHandling.All
                    var data = JsonConvert.DeserializeObject<Dictionary<string, ServerCharacterListByAccount>>(contents);
                    charMgr.AppendCharacters(data);
                }
            }
            return charMgr;
        }
        public static List<string> EnumerateCharacterFilepaths()
        {
            List<string> filepaths = new List<string>();
            string xpath = FileLocations.GetCharacterFilePath(ServerName: GameRepo.Game.Server, AccountName: GameRepo.Game.Account);
            string chardir = System.IO.Path.GetDirectoryName(xpath);
            FileLocations.EnsureFolderExists(chardir);
            foreach (string charfilename in Directory.GetFiles(chardir))
            {
                string path = Path.Combine(chardir, charfilename);
                filepaths.Add(path);
            }
            return filepaths;
        }
    }
}
