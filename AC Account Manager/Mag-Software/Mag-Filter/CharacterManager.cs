using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MagFilter
{
    public class CharacterBook
    {
        private Dictionary<string, ServerCharacterListByAccount> _data = null;

        private CharacterBook(Dictionary<string, ServerCharacterListByAccount> dictionary)
        {
            _data = dictionary;
        }
        private CharacterBook()
        {
            _data = new Dictionary<string, ServerCharacterListByAccount>();
        }
        public IEnumerable<string> GetKeys()
        {
            log.WriteDebug("GetKeys function: " + _data.Keys);
            return _data.Keys;
        }
        public ServerCharacterListByAccount GetCharacters(string serverName, string accountName)
        {
            string key = GetKey(server: serverName, accountName: accountName);
            if (this._data.ContainsKey(key))
            {
                log.WriteDebug("GetChars sN aN Function: " + this._data[key]);
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
            log.WriteDebug("GetChars key Function: {0}", _data[key]);
            return this._data[key];
        }
        private static string GetKey(string server, string accountName)
        {
            string key = string.Format("{0}-{1}", server, accountName);
            log.WriteDebug("GetKey function: {0}", key);
            return key;
        }

        public void WriteCharacters(string zonename, List<Character> characters)
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
            log.WriteDebug("LaunchInfo valid");

            // Pass info to Heartbeat
            Heartbeat.RecordServer(launchInfo.ServerName);
            Heartbeat.RecordAccount(launchInfo.AccountName);
            GameRepo.Game.SetServerAccount(server: launchInfo.ServerName, account: launchInfo.AccountName);

            string key = GetKey(server: launchInfo.ServerName, accountName: launchInfo.AccountName);
            var clist = new ServerCharacterListByAccount()
                {
                    ZoneId = zonename,
                    CharacterList = characters
                };
            log.WriteInfo("Writing characters: " + clist.ToString());
            this._data[key] = clist;
            string contents = JsonConvert.SerializeObject(_data, Formatting.Indented);
            string path = FileLocations.GetCharacterFilePath();
            using (var file = new StreamWriter(path, append: false))
            {
                file.Write(contents);
            }
        }

        private bool IsValidCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) { return false; }
            if (characterName == "None") { return false; }
            return true;
        }

        public static CharacterBook ReadCharacters()
        {
            try
            {
                log.WriteDebug("ReadCharacterImpl: " + ReadCharactersImpl());
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
            string path = FileLocations.GetCharacterFilePath();
            if (!File.Exists(path))
            {
                path = FileLocations.GetOldCharacterFilePath();
            }

            if (!File.Exists(path)) { return new CharacterBook(); }
            using (var file = new StreamReader(path))
            {
                string contents = file.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, ServerCharacterListByAccount>>(contents);
                CharacterBook charMgr = new CharacterBook(data);
                return charMgr;
            }
        }
    }
}
