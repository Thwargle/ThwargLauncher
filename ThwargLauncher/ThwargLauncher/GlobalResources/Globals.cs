using System;

namespace ThwargLauncher.GlobalResources
{
    class Globals
    {
        public static bool IsSimple {  get { return Properties.Settings.Default.LastUsedSimpleLaunch; } }
        public static bool NeverKillClients { get { return Properties.Settings.Default.NeverKillClients; } }
    }
}
