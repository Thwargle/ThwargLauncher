using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagFilter
{
    class SettingsCollection
    {
        private Dictionary<string, Setting> Settings = new Dictionary<string, Setting>();
        public void AddSetting(Setting setting)
        {
            Settings[setting.Name] = setting;
        }
        public bool ContainsKey(string key) { return Settings.ContainsKey(key); }
        public Setting GetValue(string key)
        {
            if (!Settings.ContainsKey(key)) { throw new Exception(string.Format("Missing key '{0}'", key)); }
            return Settings[key];
        }
    }

    class SettingsFileParser
    {
        public SettingsCollection ReadSettingsFile(string filepath)
        {
            var settings = new SettingsCollection();
            if (string.IsNullOrEmpty(filepath)) { throw new Exception("ReadSettingsFile received empty filename"); }
            if (!File.Exists(filepath)) { throw new Exception("Missing file: " + filepath); }
            using (var file = new StreamReader(filepath))
            {
                string contents = file.ReadToEnd();
                string[] stringSeps = new string[] { "\r\n" };
                string[] lines = contents.Split(stringSeps, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    var lineParser = new SettingsLineParser();
                    Setting setting = lineParser.ExtractLine(line);
                    settings.AddSetting(setting);
                }
            }
            return settings;
        }
    }
}
