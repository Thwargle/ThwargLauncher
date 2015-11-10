using System.Collections.Generic;
using System.Linq;

namespace AC_Account_Manager
{
    class LaunchSorter
    {
        /// <summary>
        /// Info for one game launch
        /// </summary>
        public class LaunchItem
        {
            public string AccountName;
            public string Password;
            public string ServerName;
            public string CharacterSelected;
        }
        /// <summary>
        /// List of launch items sorted for performance
        /// </summary>
        public class LaunchList
        {
            private readonly List<LaunchItem> _launchItems;
            public LaunchList() { _launchItems = new List<LaunchItem>(); }
            public void Add(LaunchItem item) { _launchItems.Add(item); }
            public IEnumerable<LaunchItem> GetLaunchList() { return _launchItems; }
            public int GetLaunchItemCount() { return _launchItems.Count; }
        }
        /// <summary>
        /// Construct a launch list from the model account info, and sort it for optimal performance
        /// </summary>
        public LaunchList GetLaunchList(List<UserAccount> accountList)
        {
            var launchList = GetLaunchListFromAccountList(accountList);
            var optimizedList = GetOptimizedLaunchList(launchList);
            return optimizedList;
        }
        private LaunchList GetLaunchListFromAccountList(List<UserAccount> accountList)
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
        /// <summary>
        /// Reorder launch items by starting with account with most launches
        /// and then rotating through accounts from most launches to least launches
        /// </summary>
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
        /// <summary>
        /// Remove & return first item on list
        /// </summary>
        private LaunchItem PopFirst(List<LaunchItem> list)
        {
            var item = list[0];
            list.RemoveAt(0);
            return item;
        }
    }
}
