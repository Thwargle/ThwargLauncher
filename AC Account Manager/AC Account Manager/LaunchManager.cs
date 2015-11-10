using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AC_Account_Manager
{
    class LaunchManager
    {
        public class LaunchItem
        {
            public string AccountName;
            public string Password;
            public string ServerName;
            public string CharacterSelected;
        }
        public class LaunchList
        {
            private readonly List<LaunchItem> _launchItems;
            public LaunchList() { _launchItems = new List<LaunchItem>(); }
            public void Add(LaunchItem item) { _launchItems.Add(item); }
            public IEnumerable<LaunchItem> GetLaunchList() { return _launchItems; }
            public int GetLaunchItemCount() { return _launchItems.Count; }
        }
        public LaunchList GetLaunchList(List<UserAccount> accountList)
        {
            var launchList = GetLaunchListFromAccountList(accountList);
            var optimizedList = GetOptimizedLaunchList(launchList);
            return optimizedList;
        }
        public LaunchList GetLaunchListFromAccountList(List<UserAccount> accountList)
        {
            var launchList = new LaunchList();
            foreach (var account in accountList)
            {
                if (account.AccountLaunchable)
                {
                    foreach (var server in account.Servers)
                    {
                        if (server.ServerSelected)
                        {
                            var launchItem = new LaunchItem()
                                {
                                    AccountName = account.Name,
                                    Password = account.Password,
                                    ServerName = server.ServerName,
                                    CharacterSelected = server.ChosenCharacter
                                };
                            launchList.Add(launchItem);
                        }
                    }
                }
            }
            return launchList;
        }
        private LaunchList GetOptimizedLaunchList(LaunchList originalList)
        {
            // Bin all items by account
            Dictionary<string, List<LaunchItem>> launchItemsByAccountName = new Dictionary<string, List<LaunchItem>>();
            foreach (LaunchItem item in originalList.GetLaunchList())
            {
                string key = item.AccountName;
                if (!launchItemsByAccountName.ContainsKey(key))
                {
                    launchItemsByAccountName[key] = new List<LaunchItem>();
                }
                launchItemsByAccountName[key].Add(item);
            }
            // Sort bins by # items
            var sortedLists = launchItemsByAccountName.Values.OrderByDescending(x => x.Count).ToList();
            // Build list starting with most populated bin, and taking one per bin until all taken
            int i = 0;
            var sortedLaunchList = new LaunchList();
            while (sortedLists.Count > 0)
            {
                if (sortedLists[i].Count > 0)
                {
                    sortedLaunchList.Add(PopFirst(sortedLists[i]));
                }
                if (sortedLists[i].Count == 0)
                {
                    sortedLists.RemoveAt(i);
                }
                ++i;
                if (i >= sortedLists.Count)
                {
                    i = 0;
                }
            }
            return sortedLaunchList;
        }
        private LaunchItem PopFirst(List<LaunchItem> list)
        {
            var item = list[0];
            list.RemoveAt(0);
            return item;
        }
    }
}
