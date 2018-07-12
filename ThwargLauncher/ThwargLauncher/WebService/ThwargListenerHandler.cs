using System;
using System.Collections.Generic;

namespace ThwargLauncher.WebService
{
    class ThwargListenerHandler : IThwargListener
    {
        public List<GameSetting> GetConfigurationSettings(string Account, string Server)
        {
            List<GameSetting> settings = new List<GameSetting>();
            settings.Add(new GameSetting() {Name = "Hi", Value = "World"});
            return settings;
        }

        public List<GameCommand> CheckIn(string Account, string Server)
        {
            List<GameCommand> commands = new List<GameCommand>();
            commands.Add(new GameCommand() {Command = "Say hello"});
            return commands;
        }
    }
}
