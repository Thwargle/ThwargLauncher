using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AC_Account_Manager
{
    class AccountParser
    {
        public static string AccountFilePath = Path.Combine(Configuration.AppFolder, "Accounts.txt");
        private const string HeaderComment = @"# Name=xxx,Password=xxx,LaunchPath=c:\xxx,PreferenceFile=c:\xxx";

        public List<UserAccount> ReadOrMigrateAccounts(string oldUsersFilePath)
        {
            var characterMgr = MagFilter.CharacterManager.ReadCharacters();
            var acctList = new List<UserAccount>();
            if (File.Exists(AccountFilePath))
            {
                acctList = ReadAccounts(characterMgr);
            }
            else if (File.Exists(oldUsersFilePath))
            {
                acctList = ReadOldUserNames(characterMgr, oldUsersFilePath);
                WriteAccounts(acctList);
            }
            else
            {
                // Ensure characters file exists
                CreateEmptyTextFile(AccountFilePath);
            }
            return acctList;
        }

        private List<UserAccount> ReadAccounts(MagFilter.CharacterManager characterMgr)
        {
            var acctList = new List<UserAccount>();
            string fileVersion = null;
            using (var reader = new StreamReader(AccountFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("#") || line.StartsWith("'")) { continue; }
                        if (fileVersion == null)
                        {
                            if (!line.StartsWith("Version="))
                            {
                                throw new Exception("Bad account file, first line not Version");
                            }
                            if (!line.StartsWith("Version=1"))
                            {
                                throw new Exception("Bad account file, Version not 1");
                            }
                            fileVersion = line.Substring("Version=".Length);
                        }
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        var nameValueSet = ParseLineIntoAttributeSet(line);
                        if (nameValueSet == null) { continue; }
                        if (!nameValueSet.ContainsKey("Name")) { continue; }
                        if (!nameValueSet.ContainsKey("Password")) { continue; }
                        string accountName = nameValueSet["Name"];
                        string password = nameValueSet["Password"];
                        string launchPath = GetSetValue(nameValueSet, "LaunchPath");
                        string preferencePath = GetSetValue(nameValueSet, "PreferencePath");

                        var user = new UserAccount(accountName, characterMgr)
                        {
                            Password = password
                        };
                        if (launchPath != null) { user.CustomLaunchPath = launchPath; }
                        if (preferencePath != null) { user.CustomPreferencePath = preferencePath; }
                        acctList.Add(user);
                    }
                }
            }
            return acctList;
        }
        private Dictionary<string, string> ParseLineIntoAttributeSet(string line)
        {
            string[] arr = line.Split(',');
            if (arr.Length < 2) { return null; }
            var nameValueSet = new Dictionary<string, string>();
            foreach (string pairstring in arr)
            {
                KeyValuePair<string, string> pair = ParseIntoPair(pairstring);
                if (!string.IsNullOrEmpty(pair.Key))
                {
                    nameValueSet[pair.Key] = pair.Value;
                }
            }
            return nameValueSet;
        }

        private KeyValuePair<string, string> ParseIntoPair(string pairstring)
        {
            var pair = new KeyValuePair<string, string>();
            int index = pairstring.IndexOf('=');
            if (index == -1) { return pair; }
            if (index == 0) { return pair; }
            string key = pairstring.Substring(0, index);
            string value = "";
            if (index < pairstring.Length - 1)
            {
                value = pairstring.Substring(index + 1);
            }
            pair = new KeyValuePair<string, string>(key, value);
            return pair;

        }
        private string GetSetValue(Dictionary<string, string> set, string name, string defval = null)
        {
            if (set.ContainsKey(name))
            {
                return set[name];
            }
            else
            {
                return defval;
            }
        }
        private void CreateEmptyTextFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                using (var fileStream = File.Create(filepath))
                {
                    // deliberately creating empty file
                }
            }
        }

        public void WriteAccounts(IEnumerable<UserAccount> acctList)
        {
            using (StreamWriter writer = new StreamWriter(AccountFilePath))
            {
                writer.WriteLine(HeaderComment);
                writer.WriteLine("Version=1");
                foreach (var acct in acctList)
                {
                    string line = string.Format(
                        "Name={0},Password={1}",
                        acct.Name, acct.Password);
                    if (!string.IsNullOrEmpty(acct.CustomLaunchPath))
                    {
                        line += string.Format(",LaunchPath={0}", acct.CustomLaunchPath);
                    }
                    if (!string.IsNullOrEmpty(acct.CustomLaunchPath))
                    {
                        line += string.Format(",PreferencePath={0}", acct.CustomPreferencePath);
                    }
                    writer.WriteLine(line);
                }
            }
        }
        private List<UserAccount> ReadOldUserNames(MagFilter.CharacterManager characterMgr, string oldUsersFilePath)
        {
            var acctList = new List<UserAccount>();
            using (var reader = new StreamReader(oldUsersFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        string[] arr = line.Split(',');
                        if (arr.Length != 2) { continue; }
                        string accountName = arr[0];
                        string password = arr[1];

                        var user = new UserAccount(accountName, characterMgr)
                            {
                                Password = password
                            };
                        acctList.Add(user);
                    }
                }
            }
            return acctList;
        }
    }
}
