using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Mag.Shared;

namespace MagFilter
{
    class MagFilterCommandManager
    {
        private delegate void ExecuteCommand(string command);
        private class CommandEntry
        {
            public readonly string Command;
            public readonly ExecuteCommand CommandHandler;
            public CommandEntry(string cmd, ExecuteCommand cmdHandler) { this.Command = cmd; this.CommandHandler = cmdHandler; }
        }
        private List<CommandEntry> commandHandlers = new List<CommandEntry>();
        // MagFilter commands. All are prefixed with "/mf "
        private const string CMD_Version = "version";
        private const string CMD_Help = "help";
        private const string CMD_Help2 = "/?";
        private const string CMD_Broadcast = "broadcast ";
        private const string CMD_Test = "test ";
        // TODO - add team commands
        public MagFilterCommandManager()
        {
            commandHandlers.Add(new CommandEntry(CMD_Version, VersionCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Help, HelpCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Help2, HelpCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Broadcast, BroadcastCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Test, TestCommandHandler));
        }
        public void FilterCore_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            string commandString = "";
            foreach (CommandEntry cmdEntry in commandHandlers)
            {
                string prefix = "/mf " + cmdEntry.Command;
                if (CheckCommandPrefix(e.Text, prefix, ref commandString))
                {
                    cmdEntry.CommandHandler(commandString);
                    e.Eat = true;
                    break;
                }
            }
        }
        private void VersionCommandHandler(string command)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string msg = string.Format(
                "MagFilter, AssemblyVer: {0}, AssemblyFileVer: {1}",
                assembly.GetName().Version,
                System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)
                                );

            Debug.WriteToChat("Version: " + msg);
            log.WriteLogMsg("Called Debug.WriteToChat Version: " + msg);
        }
        private void HelpCommandHandler(string command)
        {
            List<string> cmds = new List<string>();
            foreach (CommandEntry cmdEntry in commandHandlers)
            {
                cmds.Add(cmdEntry.Command);
            }
            Debug.WriteToChat("Commands: " + string.Join(",", cmds.ToArray()));
        }
        private void BroadcastCommandHandler(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                Heartbeat.SendCommand(command);
                Heartbeat.SendAndReceiveImmediately();
            }
        }
        private void TestCommandHandler(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                DecalProxy.DispatchChatToBoxWithPluginIntercept(command);
            }
        }
        private bool CheckCommandPrefix(string line, string prefix, ref string command)
        {
            if (line.StartsWith(prefix))
            {
                if (line.Length > prefix.Length)
                {
                    command = line.Substring(prefix.Length, line.Length - prefix.Length);
                }
                else
                {
                    command = "";
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
