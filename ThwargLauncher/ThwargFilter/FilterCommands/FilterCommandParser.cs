using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Filter.Shared;

namespace ThwargFilter
{
    class ThwargFilterCommandParser
    {
        private delegate void ExecuteCommand(string command);
        private class CommandEntry
        {
            public readonly string Command;
            public readonly ExecuteCommand CommandHandler;
            public readonly string Help;
            public CommandEntry(string cmd, ExecuteCommand cmdHandler, string help)
            {
                this.Command = cmd;
                this.CommandHandler = cmdHandler;
                this.Help = help;
            }
        }
        private class CommandEntryList : List<CommandEntry>
        {
            public void Add(string cmdString, ExecuteCommand cmdHandler, string help)
            {
                this.Add(new CommandEntry(cmd: cmdString, cmdHandler: cmdHandler, help: help));
            }
        }
        // Member variables
        private CommandEntryList cmdHandlers = new CommandEntryList();
        private ThwargFilterCommandExecutor executor;
        private Dictionary<string, int> myTeams = new Dictionary<string, int>();
        // ThwargFilter commands. All are prefixed with "/tf "
        private const string CMD_Version = "version";
        private const string CMD_Help = "help";
        private const string CMD_Help2 = "?";
        private const string CMD_Help3 = "/?";
        private const string CMD_Broadcast = "broadcast ";
        private const string CMD_Broadcast2 = "bc ";
        private const string CMD_CreateTeam = "createteam ";
        private const string CMD_CreateTeam2 = "ct ";
        private const string CMD_ShowTeams = "showteams";
        private const string CMD_ShowTeams2 = "st";
        private const string CMD_JoinTeam = "jointeam ";
        private const string CMD_JoinTeam2 = "jt ";
        private const string CMD_LeaveTeam = "leaveteam ";
        private const string CMD_LeaveTeam2 = "lt ";
        private const string CMD_Test = "test ";
        private const string CMD_SetWindowTitle = "swt ";
        private const string CMD_Inventory = "inventory";
        private const string CMD_Inventory2 = "inv";
        private const string CMD_KillClient = "killclient";
        private const string CMD_KillClient2 = "kc";
        private const string CMD_KillAllClients = "killallclients";
        private const string CMD_KillAllClients2 = "kac";
        private ThwargInventory _thwargInventory;
        public ThwargInventory Inventory { set { _thwargInventory = value; } }

        public string GetTeamList() { return GetTeamStringList(); }
        private string GetTeamStringList()
        {
            string[] teams = new string[myTeams.Count];
            myTeams.Keys.CopyTo(teams, 0);
            return string.Join(",", teams);
        }
        public ThwargFilterCommandParser(ThwargFilterCommandExecutor cmdExecutor)
        {
            executor = cmdExecutor;
            cmdHandlers.Add(CMD_Version, VersionCommandHandler, "Display assembly version info");
            cmdHandlers.Add(CMD_Help, HelpCommandHandler, "List all mf commands");
            cmdHandlers.Add(CMD_Help2, HelpCommandHandler, null);
            cmdHandlers.Add(CMD_Help3, HelpCommandHandler, null);
            cmdHandlers.Add(CMD_Broadcast, BroadcastCommandHandler, "Broadcast command to other games ('/tf bc /t:red *bow*)");
            cmdHandlers.Add(CMD_Broadcast2, BroadcastCommandHandler, "Broadcast command to other games ('/tf bc /t:red *bow*)");
            cmdHandlers.Add(CMD_CreateTeam, CreateTeamCommandHandler, "Create team of specified characters ('/tf ct Tom Bob')");
            cmdHandlers.Add(CMD_CreateTeam2, CreateTeamCommandHandler, null);
            cmdHandlers.Add(CMD_ShowTeams, ListTeamsCommandHandler, "Show all my teams ('/tf st')");
            cmdHandlers.Add(CMD_ShowTeams2, ListTeamsCommandHandler, null);
            cmdHandlers.Add(CMD_JoinTeam, JoinTeamCommandHandler, "Join a team ('/tf jt red')");
            cmdHandlers.Add(CMD_JoinTeam2, JoinTeamCommandHandler, null);
            cmdHandlers.Add(CMD_LeaveTeam, LeaveTeamCommandHandler, "Leave a team ('/tf lt red')");
            cmdHandlers.Add(CMD_LeaveTeam2, LeaveTeamCommandHandler, null);
            cmdHandlers.Add(CMD_Test, TestCommandHandler, "Test submitting a command directly to game ('/tf test somecommandstring')");
            cmdHandlers.Add(CMD_SetWindowTitle, SetWindowTitleCommandHandler, "Set window title ('/tf swt MyGame')");
            cmdHandlers.Add(CMD_Inventory, InventoryCommandHandler, "List inventory to log ('/tf inv')");
            cmdHandlers.Add(CMD_Inventory2, InventoryCommandHandler, null);
            cmdHandlers.Add(CMD_KillClient, KillClientCommandHandler, "Kill current client ('/tf kc')");
            cmdHandlers.Add(CMD_KillClient2, KillClientCommandHandler, null);
            cmdHandlers.Add(CMD_KillAllClients, KillAllClientsCommandHandler, "Kill current client ('/tf kac')");
            cmdHandlers.Add(CMD_KillAllClients2, KillAllClientsCommandHandler, null);
        }
        public void ExecuteCommandFromLauncher(string command)
        {
            string commandString = "";
            if (IsCommandPrefix(command, CMD_JoinTeam, out commandString)
                || IsCommandPrefix(command, CMD_JoinTeam2, out commandString))
            {
                JoinTeamCommandHandler(commandString);
            }
            else if (IsCommandPrefix(command, CMD_LeaveTeam, out commandString)
                || IsCommandPrefix(command, CMD_LeaveTeam2, out commandString))
            {
                LeaveTeamCommandHandler(commandString);
            }
            else
            {
                executor.ExecuteCommand(command);
            }
        }
        public void FilterCore_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            foreach (CommandEntry cmdEntry in cmdHandlers)
            {
                // Only look at commands with handlers (others are just for help display)
                if (cmdEntry.CommandHandler != null)
                {
                    string prefix = "/tf " + cmdEntry.Command;
                    string commandString;
                    if (IsCommandPrefix(e.Text, prefix, out commandString))
                    {
                        cmdEntry.CommandHandler(commandString);
                        e.Eat = true;
                        break;
                    }
                }
            }
        }
        private void VersionCommandHandler(string command)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string msg = string.Format(
                "ThwargFilter, AssemblyVer: {0}, AssemblyFileVer: {1}",
                assembly.GetName().Version,
                System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)
                                );

            Debug.WriteToChat("Version: " + msg);
            log.WriteInfo("Called Debug.WriteToChat Version: " + msg);
        }
        private void HelpCommandHandler(string command)
        {
            List<string> cmds = new List<string>();
            foreach (CommandEntry cmdEntry in cmdHandlers)
            {
                if (cmdEntry.Help != null)
                {
                    string cmdString = cmdEntry.Command.Trim();
                    cmds.Add(string.Format("{0}: {1}", cmdString, cmdEntry.Help));
                }
            }
            Debug.WriteToChat("Commands: " + string.Join("\n", cmds.ToArray()));
        }
        private void BroadcastCommandHandler(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                Heartbeat.SendCommand(CMD_Broadcast + command);
                Heartbeat.SendAndReceiveImmediately();
            }
        }
        private void CreateTeamCommandHandler(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                Heartbeat.SendCommand(CMD_CreateTeam + command);
                Heartbeat.SendAndReceiveImmediately();
            }
        }
        private void ListTeamsCommandHandler(string command)
        {
            Debug.WriteToChat("Teams: " + GetTeamStringList());
        }
        private void JoinTeamCommandHandler(string command)
        {
            foreach (string team in command.Split(new char[0], StringSplitOptions.RemoveEmptyEntries))
            {
                JoinTeam(team);
            }
        }
        private void JoinTeam(string team)
        {
            if (!myTeams.ContainsKey(team))
            {
                myTeams.Add(team, 1);
            }
        }
        private void LeaveTeamCommandHandler(string command)
        {
            foreach (string team in command.Split(new char[0], StringSplitOptions.RemoveEmptyEntries))
            {
                LeaveTeam(team);
            }
        }
        private void LeaveTeam(string team)
        {
            myTeams.Remove(team);
        }
        private void InventoryCommandHandler(string command)
        {
            _thwargInventory.HandleInventoryCommand();
        }
        private void KillClientCommandHandler(string command)
        {
            Heartbeat.SendCommand(CMD_KillClient);
            Heartbeat.SendAndReceiveImmediately();
        }
        private void KillAllClientsCommandHandler(string command)
        {
            Heartbeat.SendCommand(CMD_KillAllClients);
            Heartbeat.SendAndReceiveImmediately();
        }
        private void TestCommandHandler(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                executor.ExecuteCommand(command);
            }
        }
        private void SetWindowTitleCommandHandler(string command)
        {
            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var process = System.Diagnostics.Process.GetProcessById(pid);
            var hwnd = process.MainWindowHandle;
            string pattern = command;
            pattern = pattern.Replace("%ACCOUNT%", GameRepo.Game.Account);
            pattern = pattern.Replace("%SERVER%", GameRepo.Game.Server);
            pattern = pattern.Replace("%CHARACTER%", GameRepo.Game.Character);
            WinUtil.WinEnum.SetWindowText(hwnd, pattern);
        }
        private bool IsCommandPrefix(string line, string prefix, out string command)
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
                command = "";
                return false;
            }
        }
    }
}
