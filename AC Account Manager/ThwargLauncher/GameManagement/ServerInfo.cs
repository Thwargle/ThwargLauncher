using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ThwargLauncher
{
    public class ServerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public override bool Equals(object obj)
        {
            ServerInfo ob2 = (obj as ServerInfo);
            if (ob2 == null) { return false; }
            if (GetHashCode() != ob2.GetHashCode()) { return false; }
            if (ServerName != ob2.ServerName) { return false; }
            if (ServerIpAndPort != ob2.ServerIpAndPort) { return false; }
            return true;
        }
        public override int GetHashCode()
        {
            return ServerIpAndPort.GetHashCode();
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ServerName { get; set; }
        public string ServerDescription { get; set; }
        public bool ServerLoginEnabled { get; set; }
        public string ServerIpAndPort { get; set; }
        public string EMU { get; set; }
        public string RodatSetting { get; set; }
        private string _connectionStatus = "?";
        private System.Windows.Media.SolidColorBrush _connectionColor = System.Windows.Media.Brushes.AntiqueWhite;
        private ServerUpStatus _upStatus;
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged("ConnectionStatus");
                }
            }
        }
        public System.Windows.Media.SolidColorBrush ConnectionColor
        {
            get { return _connectionColor; }
            set
            {
                if (_connectionColor != value)
                {
                    _connectionColor = value;
                    OnPropertyChanged("ConnectionColor");
                }
            }
        }
        public enum ServerUpStatus { Unknown, Down, Up };
        public ServerUpStatus UpStatus
        {
            get { return _upStatus; }
            set
            {
                if (_upStatus != value)
                {
                    _upStatus = value;
                    ConnectionColor = GetBrushColorFromUpStatus(_upStatus);
                    OnPropertyChanged("UpStatus");
                }
            }
        }
        private System.Windows.Media.SolidColorBrush GetBrushColorFromUpStatus(ServerUpStatus upStatus)
        {
            switch (_upStatus)
            {
                case ServerUpStatus.Down: return System.Windows.Media.Brushes.Red;
                case ServerUpStatus.Up: return System.Windows.Media.Brushes.Lime;
                default: return System.Windows.Media.Brushes.AntiqueWhite;
            }
        }
    }
}
