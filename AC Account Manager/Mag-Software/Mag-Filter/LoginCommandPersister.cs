using GenericSettingsFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagFilter
{
    internal class LoginCommandPersister
    {
        private string _accountName;
        private string _serverName;
        private string _characterName;

        private static List<string> QueueToList_unused(Queue<string> queue)
        {
            List<string> list = new List<string>();
            foreach (var cmd in queue)
            {
                list.Add(cmd);
            }
            return list;
        }
        internal LoginCommandPersister()
        {
        }
        internal LoginCommandPersister(string accountName, string serverName, string characterName)
        {
            _accountName = accountName;
            _serverName = serverName;
            _characterName = characterName;
        }
        internal void WriteQueue(LoginCommands loginCommands, bool global)
        {
            string filepath = GetFilepath(global);
            using (var file = new StreamWriter(filepath, append: false))
            {
                file.WriteLine("WaitMilliseconds:{0}", loginCommands.GetInternalWaitValue());
                file.WriteLine("CommandCount:{0}", loginCommands.Commands.Count);
                int i = 0;
                foreach (string cmd in loginCommands.Commands)
                {
                    file.WriteLine("Command{0}:{1}", i, cmd);
                    ++i;
                }
            }
        }
        internal LoginCommands ReadAndCombineQueues()
        {
            try
            {
                log.WriteDebug("reading queues");
                var gcmds = ReadQueue(true);
                var cmds = ReadQueue(false);
                if (!gcmds.IsWaitSpecified() && cmds.IsWaitSpecified())
                {
                    gcmds.WaitMillisencds = cmds.WaitMillisencds;
                }
                foreach (string cmd in cmds.Commands)
                {
                    gcmds.Commands.Enqueue(cmd);
                }
                log.WriteInfo("Found {0} cmds", gcmds.Commands.Count);
                return gcmds;
            }
            catch (Exception exc)
            {
                log.WriteError("Exception in ReadQueue: {0}", exc);
                return new LoginCommands();
            }
        }
        internal LoginCommands ReadQueue(bool global)
        {
            var loginCommands = new LoginCommands();
            string filepath = GetFilepath(global);
            if (File.Exists(filepath))
            {
                var settings = (new SettingsFileParser()).ReadSettingsFile(filepath);
                loginCommands.WaitMillisencds = int.Parse(settings.GetValue("WaitMilliseconds").SingleParameter);
                int count = int.Parse(settings.GetValue("CommandCount").SingleParameter);
                for (int i = 0; i < count; ++i)
                {
                    string cmd = settings.GetValue(string.Format("Command{0}", i)).SingleParameter;
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        log.WriteInfo("cmd: '" + cmd + "'");
                    }
                    loginCommands.Commands.Enqueue(cmd);
                }
            }
            return loginCommands;
        }
        private string GetFilepath(bool global)
        {
            string filename = "";
            if (global)
            {
                filename = "LoginCommandsGlobal.txt";
            }
            else
            {
                filename = string.Format("LoginCommands-{0}-{1}-{2}.txt", _accountName, _serverName, _characterName);
                // TODO - encode to ASCII
            }

            return Path.Combine(FileLocations.GetLoginCommandsFolder(), filename);
        }
    }
    /// <summary>
    /// Called from Launcher
    /// </summary>
    public class LoginCommandsStorage
    {
        public static LoginCommands GetGlobalLoginCommands()
        {
            var persister = new LoginCommandPersister();
            var cmds = persister.ReadQueue(global:true);
            return cmds;
        }
        public static LoginCommands GetLoginCommands(string accountName, string serverName, string characterName)
        {
            var persister = new LoginCommandPersister(accountName: accountName, serverName: serverName, characterName: characterName);
            var cmds = persister.ReadQueue(global: false);
            return cmds;
        }
    }
}
