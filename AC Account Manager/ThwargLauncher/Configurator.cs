using System;
using System.Collections.Generic;
using System.Linq;

namespace ThwargLauncher
{
    /// <summary>
    /// Historical record of game configurations seen in this session
    /// </summary>
    public class Configurator
    {
        public class GameConfig
        {
            public string MagFilterPath;
            public string MagFilterVersion;
        }

        private Dictionary<string, GameConfig> _map = new Dictionary<string, GameConfig>();
        public bool ContainsMagFilterPath(string path) { return _map.ContainsKey(path); }
        public void AddGameConfig(GameConfig config)
        {
            string key = GetConfigKey(config);
            if (!_map.ContainsKey(key))
            {
                _map[key] = config;
            }
        }
        public IList<GameConfig> GetGameConfigs()
        {
            return _map.Values.ToList();
        }
        private string GetConfigKey(GameConfig config)
        {
            return config.MagFilterPath.ToUpperInvariant();
        }
    }
}
