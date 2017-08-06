using System;
using System.Configuration;

namespace ThwargLauncher
{
    class ConfigSettings
    {
        public static string GetConfigString(string name, string defval)
        {
            string text = ConfigurationManager.AppSettings[name];
            if (text == null) { return defval; }
            return text;
        }
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
        public static bool GetConfigBool(string name, bool defval)
        {
            string text = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(text)) { return defval; }
            return PersistenceHelper.AppSettings.ObjToBool(text, defval);
        }
    }
}
