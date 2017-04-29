using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Media;
using System.Net;
using System.Security.Policy;
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
        public enum ServerUpStatusEnum
        {
            Unknown,
            Down,
            Up
        };

        public enum ServerSourceEnum
        {
            User,
            Published
        };

        public enum ServerEmuEnum
        {
            Phat,
            Ace
        };

        public enum RodatEnum
        {
            On,
            Off
        };

        public enum VisibilityEnum
        {
            Visible,
            Invisible
        };

        public override bool Equals(object obj)
        {
            ServerModel ob2 = (obj as ServerModel);
            if (ob2 == null)
            {
                return false;
            }
            if (this.ServerId == ob2.ServerId)
            {
                return true;
            }
            /*
             * We are now using exact id match, not equivalent data
            if (GetHashCode() != ob2.GetHashCode()) { return false; }
            if (ServerName != ob2.ServerName) { return false; }
            if (ServerIpAndPort != ob2.ServerIpAndPort) { return false; }
             * */
            return true;
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
            server.EMU = data.EMU;
            server.RodatSetting = data.RodatSetting;
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
            if (ServerName != data.ServerName)
            {
                return false;
            }
            if (ServerIpAndPort != data.ConnectionString)
            {
                return false;
            }
            if (ServerId != data.ServerId)
            {
                return false;
            } // using exact Id match, not just equivalent data
            return true;
        }

        public string ServerName
        {
            get { return Get<string>(); }
            set
            {
                if (Set(value))
                {
                    NotifyOfPropertyChange(() => ServerDisplayAlias);
                }
            }
        }

        public string ServerAlias
        {
            get { return Get<string>(); }
            set
            {
                if (Set(value))
                {
                    NotifyOfPropertyChange(() => ServerDisplayAlias);
                }
            }
        }

        public string ServerDisplayAlias
        {
            get { return (!string.IsNullOrEmpty(ServerAlias) ? ServerAlias : ServerName); }
        }

        public string ServerDescription
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public bool ServerLoginEnabled
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        public string ServerIpAndPort
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public ServerEmuEnum EMU
        {
            get { return Get<ServerEmuEnum>(); }
            set { Set(value); }
        }

        public RodatEnum RodatSetting
        {
            get { return Get<RodatEnum>(); }
            set { Set(value); }
        }

        public VisibilityEnum VisibilitySetting
        {
            get { return Get<VisibilityEnum>(); }
            set { Set(value); }
        }

        public string ConnectionStatus
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public System.Windows.Media.SolidColorBrush ConnectionColor
        {
            get { return Get<System.Windows.Media.SolidColorBrush>(); }
            set { Set(value); }
        }

        public ServerUpStatusEnum UpStatus
        {
            get { return Get<ServerUpStatusEnum>(); }
            set
            {
                // We have to adjust ConnectionColor any time UpStatus changes
                if (Set(value))
                {
                    ConnectionColor = GetBrushColorFromUpStatus(UpStatus);
                }
            }
        }

        public ServerSourceEnum ServerSource
        {
            get { return Get<ServerSourceEnum>(); }
            set { Set(value); }
        }

        public bool IsUserServer
        {
            get { return ServerSource == ServerSourceEnum.User; }
        }

        public int StatusOfflineIntervalSeconds
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        public int StatusOnlineIntervalSeconds
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        public Guid ServerId { get; set; }

        private static System.Windows.Media.SolidColorBrush GetBrushColorFromUpStatus(ServerUpStatusEnum upStatus)
        {
            switch (upStatus)
            {
                case ServerUpStatusEnum.Down: return System.Windows.Media.Brushes.Red;
                case ServerUpStatusEnum.Up: PlayServerSound();return System.Windows.Media.Brushes.Lime;
                default: return System.Windows.Media.Brushes.AntiqueWhite;
            }
        }

        public static int PickSound;
        public static string ServerUpSound = "Audio\\DrudgeScream.wav";
        public static string Armoredillo = "Audio\\Armoredillo.wav";
        public static string Buff = "Audio\\Buff.wav";
        public static string CowMoo = "Audio\\CowMoo.wav";
        public static string DrudgeDeath = "Audio\\DrudgeDeath.wav";
        public static string DrudgeScream = "Audio\\DrudgeScream.wav";
        public static string IslandBird = "Audio\\IslandBird.wav";
        public static string ItemEnchant = "Audio\\ItemEnchant.wav";
        public static string LevelUp = "Audio\\LevelUp.wav";
        public static string LSAttune = "Audio\\LSAttune.wav";
        public static string Lugian = "Audio\\Lugian.wav";
        public static string Mattekar = "Audio\\Mattekar.wav";
        public static string MiteYeep = "Audio\\MiteYeep.wav";
        public static string Olthoi = "Audio\\Olthoi.wav";
        public static string RareFound = "Audio\\RareFound.wav";
        public static string Reedshark = "Audio\\Reedshark.wav";
        public static string Resist = "Audio\\Resist.wav";
        public static string SkillPoint = "Audio\\SkillPoint.wav";
        public static string UrsuinDeath = "Audio\\UrsuinDeath.wav";

        public static void PickNewSound()
        {
            switch (PickSound) // Add more sounds when options are completed
            {
                case 0:
                    ServerUpSound = Armoredillo;
                    return;
                case 1:
                    ServerUpSound = Buff;
                    return;
                case 2:
                    ServerUpSound = CowMoo;
                    return;
                case 3:
                    ServerUpSound = DrudgeDeath;
                    return;
                case 4:
                    ServerUpSound = DrudgeScream;
                    return;
                case 5:
                    ServerUpSound = IslandBird;
                    return;
                case 6:
                    ServerUpSound = ItemEnchant;
                    return;
                case 7:
                    ServerUpSound = LevelUp;
                    return;
                case 8:
                    ServerUpSound = LSAttune;
                    return;
                case 9:
                    ServerUpSound = Lugian;
                    return;
                case 10:
                    ServerUpSound = Mattekar;
                    return;
                case 11:
                    ServerUpSound = MiteYeep;
                    return;
                case 12:
                    ServerUpSound = Olthoi;
                    return;
                case 13:
                    ServerUpSound = RareFound;
                    return;
                case 14:
                    ServerUpSound = Reedshark;
                    return;
                case 15:
                    ServerUpSound = Resist;
                    return;
                case 16:
                    ServerUpSound = SkillPoint;
                    return;
                case 17:
                    ServerUpSound = UrsuinDeath;
                    return;
                default:
                    ServerUpSound = LevelUp;
                    break;
            }
        }

        public static void PlayServerSound()
        {
            PickSound = Properties.Settings.Default.SetServerSound;
            PickNewSound();
            SoundPlayer serverUpSound = new SoundPlayer(@ServerUpSound);
            serverUpSound.Play();
        }
    }
}
