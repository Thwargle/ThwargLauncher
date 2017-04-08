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
    /// This is the master data in memory, and various displays bind to this data
    /// </summary>
    public class ServerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public enum ServerUpStatusEnum { Unknown, Down, Up };
        public enum ServerSourceEnum { User, Published };
        public enum ServerEmuEnum { Phat, Ace };

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
        internal static ServerModel Create(ThwargLauncher.GameManagement.ServerPersister.ServerData data)
        {
            ServerModel server = new ServerModel();
            server.ServerName = data.ServerName;
            server.ServerDescription = data.ServerDesc;
            server.ServerIpAndPort = data.ConnectionString;
            server.EMU = data.EMU;
            server.RodatSetting = data.RodatSetting;
            server.ServerSource = data.ServerSource;
            return server;
        }
        internal bool IsEqual(ThwargLauncher.GameManagement.ServerPersister.ServerData data)
        {
            if (ServerName != data.ServerName) { return false; }
            if (ServerIpAndPort != data.ConnectionString) { return false; }
            return true;
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                if (_serverName != value)
                {
                    _serverName = value;
                    OnPropertyChanged("ServerName");
                }
            }
        }
        private string _serverDescription;
        public string ServerDescription
        {
            get { return _serverDescription; }
            set
            {
                if (_serverDescription != value)
                {
                    _serverDescription = value;
                    OnPropertyChanged("ServerDescription");
                }
            }
        }
        private bool _serverLoginEnabled;
        public bool ServerLoginEnabled
        {
            get { return _serverLoginEnabled; }
            set
            {
                if (_serverLoginEnabled != value)
                {
                    _serverLoginEnabled = value;
                    OnPropertyChanged("ServerLoginEnabled");
                }
            }
        }
        private string _serverIpAndPort;
        public string ServerIpAndPort
        {
            get { return _serverIpAndPort; }
            set
            {
                if (_serverIpAndPort != value)
                {
                    _serverIpAndPort = value;
                    OnPropertyChanged("ServerIpAndPort");
                }
            }
        }
        private ServerEmuEnum _emu;
        public ServerEmuEnum EMU
        {
            get { return _emu; }
            set
            {
                if (_emu != value)
                {
                    _emu = value;
                    OnPropertyChanged("EMU");
                }
            }
        }
        private string _rodatSetting;
        public string RodatSetting
        {
            get { return _rodatSetting; }
            set
            {
                if (_rodatSetting != value)
                {
                    _rodatSetting = value;
                    OnPropertyChanged("RodatSetting");
                }
            }
        }
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
