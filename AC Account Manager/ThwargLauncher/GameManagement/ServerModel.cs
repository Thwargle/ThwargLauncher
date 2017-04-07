using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ThwargLauncher
{
    /// <summary>
    /// A ServerModel is the information about one game server
    /// This is independent of accounts or runnning games
    /// </summary>
    public class ServerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public enum ServerUpStatusEnum { Unknown, Down, Up };
        public enum ServerSourceEnum { User, Published };

        public override bool Equals(object obj)
        {
            ServerModel ob2 = (obj as ServerModel);
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
        private ServerUpStatusEnum _upStatus = ServerUpStatusEnum.Unknown;
        private ServerSourceEnum _serverSource = ServerSourceEnum.User;
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
        public ServerUpStatusEnum UpStatus
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
        public ServerSourceEnum ServerSource
        {
            get { return _serverSource; }
            set
            {
                if (_serverSource != value)
                {
                    _serverSource = value;
                    OnPropertyChanged("ServerSource");
                }
            }
        }
       
        private System.Windows.Media.SolidColorBrush GetBrushColorFromUpStatus(ServerUpStatusEnum upStatus)
        {
            switch (_upStatus)
            {
                case ServerUpStatusEnum.Down: return System.Windows.Media.Brushes.Red;
                case ServerUpStatusEnum.Up: return System.Windows.Media.Brushes.Lime;
                default: return System.Windows.Media.Brushes.AntiqueWhite;
            }
        }
    }
}
