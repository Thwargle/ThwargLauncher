using System;
using System.IO;
using System.Runtime.InteropServices;

using Filter.Shared;
using Filter.Shared.Settings;

using Decal.Adapter;

namespace ThwargFilter
{
    [FriendlyName("ThwargFilter")]
    public class FilterCore : FilterBase
    {
        readonly AutoRetryLogin autoRetryLogin = new AutoRetryLogin();
        readonly LoginCharacterTools loginCharacterTools = new LoginCharacterTools();
        readonly FastQuit fastQuit = new FastQuit();
        readonly LoginCompleteMessageQueueManager loginCompleteMessageQueueManager = new LoginCompleteMessageQueueManager();
        readonly AfterLoginCompleteMessageQueueManager afterLoginCompleteMessageQueueManager = new AfterLoginCompleteMessageQueueManager();

        DefaultFirstCharacterManager defaultFirstCharacterManager;
        private LauncherChooseCharacterManager chooseCharacterManager;
        private ThwargFilterCommandExecutor ThwargFilterCommandExecutor;
        private ThwargFilterCommandParser ThwargFilterCommandParser;
        private LoginNextCharacterManager loginNextCharacterManager;
        private ThwargInventory thwargInventory;

        private DateTime _lastServerDispatchUtc = DateTime.MinValue;
        private static FilterCore theFilterCore = null;


        private string PluginName { get { return FileLocations.FilterName; } }

        public void ExternalStartup() { Startup(); } // for game emulator
        protected override void Startup()
        {
            Debug.Init(FileLocations.PluginPersonalFolder.FullName + @"\Exceptions.txt", PluginName);
            SettingsFile.Init(FileLocations.GetFilterSettingsFilepath(), PluginName);
            LogStartup();
            theFilterCore = this;

            defaultFirstCharacterManager = new DefaultFirstCharacterManager(loginCharacterTools);
            chooseCharacterManager = new LauncherChooseCharacterManager(loginCharacterTools);
            ThwargFilterCommandExecutor = new ThwargFilterCommandExecutor();
            ThwargFilterCommandParser = new ThwargFilterCommandParser(ThwargFilterCommandExecutor);
            Heartbeat.SetCommandParser(ThwargFilterCommandParser);
            loginNextCharacterManager = new LoginNextCharacterManager(loginCharacterTools);
            thwargInventory = new ThwargInventory();
            ThwargFilterCommandParser.Inventory = thwargInventory;

            ClientDispatch += new EventHandler<NetworkMessageEventArgs>(FilterCore_ClientDispatch);
            ServerDispatch += new EventHandler<NetworkMessageEventArgs>(FilterCore_ServerDispatch);
            WindowMessage += new EventHandler<WindowMessageEventArgs>(FilterCore_WindowMessage);

            CommandLineText += new EventHandler<ChatParserInterceptEventArgs>(FilterCore_CommandLineText);
        }

        public static DateTime GetLastServerDispatchUtc()
        {
            return theFilterCore._lastServerDispatchUtc;
        }

        private void LogStartup()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            log.WriteInfo(
                "ThwargFilter.Startup, AssemblyVer: {0}, AssemblyFileVer: {1}",
                assembly.GetName().Version,
                System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)
                                );
        }

        public void ExternalShutdown() { Shutdown(); } // for game emulator
        protected override void Shutdown()
        {
            ClientDispatch -= new EventHandler<NetworkMessageEventArgs>(FilterCore_ClientDispatch);
            ServerDispatch -= new EventHandler<NetworkMessageEventArgs>(FilterCore_ServerDispatch);
            WindowMessage -= new EventHandler<WindowMessageEventArgs>(FilterCore_WindowMessage);

            CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(FilterCore_CommandLineText);

            log.WriteInfo("FilterCore-Shutdown");
        }

        public void CallFilterCoreClientDispatch(object sender, NetworkMessageEventArgs e) // for game emulator
        {
            FilterCore_ClientDispatch(sender, e);
        }
        void FilterCore_ClientDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                autoRetryLogin.FilterCore_ClientDispatch(sender, e);
                loginCompleteMessageQueueManager.FilterCore_ClientDispatch(sender, e);
                afterLoginCompleteMessageQueueManager.FilterCore_ClientDispatch(sender, e);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        void FilterCore_ServerDispatch(object sender, NetworkMessageEventArgs e)
        {
            try
            {
                _lastServerDispatchUtc = DateTime.UtcNow;
                autoRetryLogin.FilterCore_ServerDispatch(sender, e);
                loginCharacterTools.FilterCore_ServerDispatch(sender, e);

                defaultFirstCharacterManager.FilterCore_ServerDispatch(sender, e);
                chooseCharacterManager.FilterCore_ServerDispatch(sender, e);
                loginNextCharacterManager.FilterCore_ServerDispatch(sender, e);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        void FilterCore_WindowMessage(object sender, WindowMessageEventArgs e)
        {
            try
            {
                fastQuit.FilterCore_WindowMessage(sender, e);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        void FilterCore_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                loginCompleteMessageQueueManager.FilterCore_CommandLineText(sender, e);
                afterLoginCompleteMessageQueueManager.FilterCore_CommandLineText(sender, e);

                defaultFirstCharacterManager.FilterCore_CommandLineText(sender, e);
                chooseCharacterManager.FilterCore_CommandLineText(sender, e);
                loginNextCharacterManager.FilterCore_CommandLineText(sender, e);
                ThwargFilterCommandParser.FilterCore_CommandLineText(sender, e);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }
}
