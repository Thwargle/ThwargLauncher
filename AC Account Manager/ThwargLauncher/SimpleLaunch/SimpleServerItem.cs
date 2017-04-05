using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThwargLauncher
{
    /// <summary>
    /// View model wrapper for server data to be bound to Simple Launch server list
    /// </summary>
    public class SimpleServerItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        
        private readonly Server.ServerItem _item;
        public SimpleServerItem(Server.ServerItem server)
        {
            _item = server;
            _item.PropertyChanged += OnItemPropertyChanged;
        }

        void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        public string ServerName { get { return _item.ServerName; } }
        public string EMU { get { return _item.EMU; } }
        public string ServerIpAndPort { get { return _item.ServerIpAndPort; } }
        public string ServerDescription { get { return _item.ServerDescription; } }
        public Server.ServerItem ServerItem { get { return _item; }}
        public string ConnectionStatus { get { return _item.ConnectionStatus; } set { _item.ConnectionStatus = value; } }

        public override bool Equals(object obj)
        {
            SimpleServerItem item2 = obj as SimpleServerItem;
            if (item2 == null) { return false; }
            return ServerName == item2.ServerName && ServerIpAndPort == item2.ServerIpAndPort;
        }
        public override int GetHashCode()
        {
            return ServerName.GetHashCode() & ServerIpAndPort.GetHashCode();
        }
    }
}
