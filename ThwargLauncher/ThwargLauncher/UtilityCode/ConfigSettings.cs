using System;
using System.Configuration;

namespace ThwargLauncher
{
    class ConfigSettings
    {
        public static string GetConfigString(string name, string defval)
        {
            try
            {
                string text = ConfigurationManager.AppSettings[name];
                if (text == null) { return defval; }
                return text;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Exception in {0}({1}): {2})",
                    new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name, name, exc);
                return defval;
            }
        }
        public static int GetConfigInt(string name, int defval)
        {
            try
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
            catch (Exception exc)
            {
                Logger.WriteError("Exception in {0}({1}): {2})",
                    new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name, name, exc);
                return defval;
            }
        }
        public static bool GetConfigBool(string name, bool defval)
        {
            try
            {
                string text = ConfigurationManager.AppSettings[name];
                if (string.IsNullOrEmpty(text)) { return defval; }
                return PersistenceHelper.AppSettings.ObjToBool(text, defval);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Exception in {0}({1}): {2})",
                    new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name, name, exc);
                return defval;
            }
        }
    }
}
