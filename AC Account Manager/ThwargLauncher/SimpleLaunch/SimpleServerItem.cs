using System;
using System.ComponentModel;

namespace ThwargLauncher
{
    /// <summary>
    /// View model wrapper for server data to be bound to Simple Launch server list
    /// </summary>
    public class SimpleServerItem
    {
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Server.ServerItem _item;
        public SimpleServerItem(Server.ServerItem server)
        {
            _item = server;
        }
        public string ServerName { get { return _item.ServerName; } }
        public string EMU { get { return _item.EMU; } }
        public string ServerIP { get { return _item.ServerIP; } }
        public string ServerDescription { get { return _item.ServerDescription; } }
        public Server.ServerItem ServerItem { get { return _item; }}

        public override bool Equals(object obj)
        {
            SimpleServerItem item2 = obj as SimpleServerItem;
            if (item2 == null) { return false; }
            return ServerName == item2.ServerName && ServerIP == item2.ServerIP;
        }
        public override int GetHashCode()
        {
            return ServerName.GetHashCode() & ServerIP.GetHashCode();
        }
    }
}
