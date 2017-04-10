using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace MagFilter
{
    class ThwargInventory
    {
        private Dictionary<int, string> _items = new Dictionary<int, string>();

        public ThwargInventory()
        {
            CoreManager.Current.ItemSelected += Current_ItemSelected;
        }

        private void Current_ItemSelected(object sender, ItemSelectedEventArgs e)
        {
            if (e.ItemGuid == 0) { return; }
            if (!_items.ContainsKey(e.ItemGuid))
            {
                log.WriteDebug("Item selected {0} - sending request", e.ItemGuid);
                CoreManager.Current.Actions.RequestId(e.ItemGuid);
            }
            else
            {
                log.WriteDebug("Item selected {0} - no request", e.ItemGuid);
            }
        }
        public void HandleInventoryCommand()
        {
            foreach (WorldObject wo in CoreManager.Current.WorldFilter.GetInventory())
            {
                if (!wo.HasIdData)
                {
                    log.WriteDebug("Lack id data for {0}", wo.Id);
                }
                else
                {
                    log.WriteDebug("Id {0}, ObjectClass {1} Name {2}", wo.Id, wo.Name, wo.ObjectClass);
                }
            }
        }
    }
}
