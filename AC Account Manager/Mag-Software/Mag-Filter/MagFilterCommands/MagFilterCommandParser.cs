using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Mag.Shared;

namespace MagFilter
{
    class MagFilterCommandParser
    {
        private delegate void ExecuteCommand(string command);
        private class CommandEntry
        {
            public readonly string Command;
            public readonly ExecuteCommand CommandHandler;
            public CommandEntry(string cmd, ExecuteCommand cmdHandler) { this.Command = cmd; this.CommandHandler = cmdHandler; }
        }
        // Member variables
        private List<CommandEntry> commandHandlers = new List<CommandEntry>();
        private MagFilterCommandExecutor executor;
        private Dictionary<string, int> myTeams = new Dictionary<string, int>();
        // MagFilter commands. All are prefixed with "/mf "
        private const string CMD_Version = "version";
        private const string CMD_Help = "help";
        private const string CMD_Help2 = "?";
        private const string CMD_Help3 = "/?";
        private const string CMD_Broadcast = "broadcast ";
        private const string CMD_CreateTeam = "createteam ";
        private const string CMD_ListTeams = "listteams";
        private const string CMD_JoinTeam = "jointeam ";
        private const string CMD_LeaveTeam = "leaveteam ";
        private const string CMD_Test = "test ";

        public string GetTeamList() { return GetTeamStringList(); }
        private string GetTeamStringList()
        {
            string[] teams = new string[myTeams.Count];
            myTeams.Keys.CopyTo(teams, 0);
            return string.Join(",", teams);
        }
        public MagFilterCommandParser(MagFilterCommandExecutor cmdExecutor)
        {
            executor = cmdExecutor;
            commandHandlers.Add(new CommandEntry(CMD_Version, VersionCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Help, HelpCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Help2, HelpCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Broadcast, BroadcastCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_CreateTeam, CreateTeamCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_ListTeams, ListTeamsCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_JoinTeam, JoinTeamCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_LeaveTeam, LeaveTeamCommandHandler));
            commandHandlers.Add(new CommandEntry(CMD_Test, TestCommandHandler));
        }
        public void ExecuteCommandFromLauncher(string command)
        {
            string commandString = "";
            if (IsCommandPrefix(command, CMD_JoinTeam, out commandString))
            {
                JoinTeamCommandHandler(commandString);
            }
            else if (IsCommandPrefix(command, CMD_LeaveTeam, out commandString))
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
            string commandString;
            foreach (CommandEntry cmdEntry in commandHandlers)
            {
                string prefix = "/mf " + cmdEntry.Command;
                if (IsCommandPrefix(e.Text, prefix, out commandString))
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
        private void TestCommandHandler(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                executor.ExecuteCommand(command);
            }
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
