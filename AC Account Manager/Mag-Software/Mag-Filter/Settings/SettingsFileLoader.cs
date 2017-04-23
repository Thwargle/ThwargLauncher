using System;
using System.Collections.Generic;
using System.IO;

namespace GenericSettingsFile
{
    /// <summary>
    /// A parser to read a text file into a SettingsCollection dictionary of named settings
    /// </summary>
    class SettingsFileLoader
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
