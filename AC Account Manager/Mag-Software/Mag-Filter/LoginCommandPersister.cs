using GenericSettingsFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagFilter
{
    class LoginCommandPersister
    {
        public void WriteQueue(LoginCommands loginCommands, bool global)
        {
            string filepath = GetFilepath(global);
            using (var file = new StreamWriter(filepath, append: false))
            {
                file.WriteLine("WaitMilliseconds:{0}", loginCommands.GetInternalWaitValue());
                file.WriteLine("CommandCount:{0}", loginCommands.MessageQueue.Count);
                int i = 0;
                foreach (string cmd in loginCommands.MessageQueue)
                {
                    file.WriteLine("Command{0}:{1}", i, cmd);
                    ++i;
                }
            }
        }
        public LoginCommands ReadAndCombineQueues()
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
                foreach (string cmd in cmds.MessageQueue)
                {
                    gcmds.MessageQueue.Enqueue(cmd);
                }
                log.WriteInfo("Found {0} cmds", gcmds.MessageQueue.Count);
                return gcmds;
            }
            catch (Exception exc)
            {
                log.WriteError("Exception in ReadQueue: {0}", exc);
                return new LoginCommands();
            }
        }
        public LoginCommands ReadQueue(bool global)
        {
            var loginCommands = new LoginCommands();
            string filepath = GetFilepath(global);
            if (File.Exists(filepath))
            {
                log.WriteDebug("qq01");
                var settings = (new SettingsFileParser()).ReadSettingsFile(filepath);
                log.WriteDebug("qq03");
                log.WriteDebug("qq05");
                loginCommands.WaitMillisencds = int.Parse(settings.GetValue("WaitMilliseconds").SingleParameter);
                int count = int.Parse(settings.GetValue("CommandCount").SingleParameter);
                for (int i = 0; i < count; ++i)
                {
                    log.WriteDebug("qq--{0} ({1})", i, global);
                    string cmd = settings.GetValue(string.Format("Command{0}", i)).SingleParameter;
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        log.WriteInfo("cmd: '" + cmd + "'");
                    }
                    loginCommands.MessageQueue.Enqueue(cmd);
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
                filename = string.Format("LoginCommands-{0}-{1}-{2}.txt", GameRepo.Game.Account, GameRepo.Game.Server, GameRepo.Game.Character);
                // TODO - encode to ASCII
            }

            return Path.Combine(FileLocations.GetLoginCommandsFolder(), filename);
        }
    }
}
