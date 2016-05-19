using System;
using System.Configuration;

namespace ThwargLauncher
{
    class ConfigSettings
    {
        public static int GetConfigInt(string name, int defval)
        {
            string text = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(text)) { return defval; }
            int value;
            if (int.TryParse(text, out value))
            {
                return value;
            }
            else
            {
                return defval;
            }
        }
        public static string GetConfigString(string name, string defval)
        {
            string text = ConfigurationManager.AppSettings[name];
            if (text == null) { return defval; }
            return text;
        }
    }
}
