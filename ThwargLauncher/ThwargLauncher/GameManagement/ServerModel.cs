using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Bindable = TwMoch.Framework.Bindable;

namespace ThwargLauncher
{
    /// <summary>
    /// A ServerModel is the information about one game server
    /// This is independent of accounts or runnning games
    /// This is the master data in memory, and various displays bind to this data
    /// </summary>
    public class ServerModel : Bindable
    {
        public enum ServerUpStatusEnum { Unknown, Down, Up };
        public enum ServerSourceEnum { User, Published };
        public enum ServerEmuEnum { GDL, ACE };
        public enum RodatEnum { On, Off };
        public enum SecureEnum { On, Off };
        public string FancyName { get { return "HI WORLD"; } }
        public enum VisibilityEnum { Visible, Invisible };
        public bool HasDiscordURL { get { return DiscordUrl != ""; } }
        public bool HasPlayerCountToolTip { get { return PlayerCountToolTip != ""; } }
        private int _playerCount = -1;
        public string Age { get; set; }
        public string PlayerCountToolTip {
            get
            {
                if (PlayerCount < 0) return "";
                return string.Format("Player Count: {0} (as of {1}) courtesy of TreeStats.net", PlayerCount, Age);
            }
        }
        public int PlayerCount
        {
            get { return _playerCount;  }
            set
            {
                if (_playerCount != value)
                {
                    _playerCount = value;
                    NotifyOfPropertyChange(() => PlayerCount);
                    NotifyOfPropertyChange(() => PlayerCountToolTip);
                    NotifyOfPropertyChange(() => HasPlayerCountToolTip);
                }

            }
        }

        public override bool Equals(object obj)
        {
            ServerModel ob2 = (obj as ServerModel);
            if (ob2 == null) { return false; }
            if (this.ServerId == ob2.ServerId) { return true; }
            /*
             * We are now using exact id match, not equivalent data
            if (GetHashCode() != ob2.GetHashCode()) { return false; }
            if (ServerName != ob2.ServerName) { return false; }
            if (ServerIpAndPort != ob2.ServerIpAndPort) { return false; }
             * */
            return false;
        }
        public override int GetHashCode()
        {
            return ServerId.GetHashCode();
            // Using exact id match, not equivalent data
            // return ServerIpAndPort.GetHashCode();
        }
        internal static ServerModel Create(ThwargLauncher.GameManagement.ServerPersister.ServerData data)
        {
            ServerModel server = new ServerModel();
            server.ServerId = data.ServerId;
            server.ServerName = data.ServerName;
            server.ServerAlias = data.ServerAlias;
            server.ServerDescription = data.ServerDesc;
            server.ServerIpAndPort = data.ConnectionString;
            server.GameApiUrl = data.GameApiUrl;
            server.LoginServerUrl = data.LoginServerUrl;
            server.DiscordUrl = data.DiscordUrl;
            server.EMU = data.EMU;
            server.RodatSetting = data.RodatSetting;
            server.SecureSetting = data.SecureSetting;
            server.VisibilitySetting = data.VisibilitySetting;
            server.ServerSource = data.ServerSource;
            server.ConnectionStatus = "?";
            server.ConnectionColor = System.Windows.Media.Brushes.AntiqueWhite;
            server.UpStatus = ServerUpStatusEnum.Unknown;
            server.StatusOfflineIntervalSeconds = 15;
            server.StatusOnlineIntervalSeconds = 300;
            return server;
        }
        internal bool IsEqual(ThwargLauncher.GameManagement.ServerPersister.ServerData data)
        {
            if (ServerName != data.ServerName) { return false; }
            if (ServerIpAndPort != data.ConnectionString) { return false; }
            if (ServerId != data.ServerId) { return false; } // using exact Id match, not just equivalent data
            return true;
        }

        public string ServerName
        {
            get { return Get<string>(); }
            set { if (Set(value)) { NotifyOfPropertyChange(() => ServerDisplayAlias); } }
        }
        public string ServerAlias
        {
            get { return Get<string>(); }
            set { if (Set(value)) { NotifyOfPropertyChange(() => ServerDisplayAlias); } }
        }
        public string ServerDisplayAlias { get { return (!string.IsNullOrEmpty(ServerAlias) ? ServerAlias : ServerName); } }
        public string ServerDescription { get { return Get<string>(); } set { Set(value); } }
        public bool ServerLoginEnabled { get { return Get<bool>(); } set { Set(value); } }
        public string ServerIpAndPort { get { return Get<string>(); } set { Set(value); } }
        public string GameApiUrl { get { return Get<string>(); } set { Set(value); } }
        public string LoginServerUrl { get { return Get<string>(); } set { Set(value); } }
        public string DiscordUrl { get { return Get<string>(); } set { Set(value); } }
        public ServerEmuEnum EMU { get { return Get<ServerEmuEnum>(); } set { Set(value); } }
        public RodatEnum RodatSetting { get { return Get<RodatEnum>(); } set { Set(value); } }
        public SecureEnum SecureSetting { get { return Get<SecureEnum>(); } set { Set(value); } }
        public VisibilityEnum VisibilitySetting { get { return Get<VisibilityEnum>(); } set { Set(value); } }
        public string ConnectionStatus { get { return Get<string>(); } set { Set(value); } }
        public System.Windows.Media.SolidColorBrush ConnectionColor { get { return Get<System.Windows.Media.SolidColorBrush>(); } set { Set(value); } }
        public ServerUpStatusEnum UpStatus
        {
            get { return Get<ServerUpStatusEnum>(); }
            set {
                // We have to adjust ConnectionColor any time UpStatus changes
                if (Set(value)) 
                {
                    ConnectionColor = GetBrushColorFromUpStatus(UpStatus);
                }
            }
        }
        public ServerSourceEnum ServerSource { get { return Get<ServerSourceEnum>(); } set { Set(value); } }
        public bool IsUserServer { get { return ServerSource == ServerSourceEnum.User; } }
        public int StatusOfflineIntervalSeconds { get { return Get<int>(); } set { Set(value); } }
        public int StatusOnlineIntervalSeconds { get { return Get<int>(); } set { Set(value); } }
        public Guid ServerId { get; set; }
       
        private static System.Windows.Media.SolidColorBrush GetBrushColorFromUpStatus(ServerUpStatusEnum upStatus)
        {
            switch (upStatus)
            {
                case ServerUpStatusEnum.Down: return System.Windows.Media.Brushes.Red;
                case ServerUpStatusEnum.Up: return System.Windows.Media.Brushes.Lime;
                default: return System.Windows.Media.Brushes.AntiqueWhite;
            }
        }
    }
}
