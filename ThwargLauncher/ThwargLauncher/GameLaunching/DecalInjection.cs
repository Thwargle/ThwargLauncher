using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace ThwargLauncher
{
    class DecalInjection
    {
        public static string GetDecalLocation()
        {
            string subKey = "SOFTWARE\\Decal\\Agent";
            try
            {
                RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(subKey);
                if (sk1 == null) { throw new Exception("Decal registry key not found: " + subKey); }

                string decalInjectionFile = (string)sk1.GetValue("AgentPath", "");
                if (string.IsNullOrEmpty(decalInjectionFile)) { throw new Exception("Decal AgentPath"); }

                decalInjectionFile += "Inject.dll";

                if (decalInjectionFile.Length > 5 && File.Exists(decalInjectionFile))
                {
                    return decalInjectionFile;
                }
            }
            catch (Exception exc)
            {
                throw new Exception("No Decal in registry: " + exc.Message);
            }
            return "NoDecal";
        }

        public static bool IsDecalInstalled()
        {
            string subKey = @"SOFTWARE\Decal\Agent";
            try
            {
                RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(subKey);
                if (sk1 == null) { return false; }
                string decalInjectionFile = (string)sk1.GetValue("AgentPath", "");
                if (string.IsNullOrEmpty(decalInjectionFile)) { return false; }
                decalInjectionFile += "Inject.dll";

                if (!File.Exists(decalInjectionFile)) { return false; }

                return true;
            }
            catch (Exception exc)
            {
                throw new Exception("No Decal in registry: " + exc.Message);
            }
        }
        public static bool IsThwargFilterRegistered()
        {
            string subKey = @"SOFTWARE\Decal\NetworkFilters\{5C60E9C9-6F53-40EB-B2BE-5E67D76414B9}";
            try
            {
                RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(subKey);
                if (sk1 == null) { return false; }
                string ThwargFilterDLL = (string)sk1.GetValue("Path", "");
                if (string.IsNullOrEmpty(ThwargFilterDLL)) { return false; }
                ThwargFilterDLL += @"\ThwargFilter.dll";

                if (!File.Exists(ThwargFilterDLL)) { return false; }

                return true;
            }
            catch (Exception exc)
            {
                throw new Exception("ThwargFilter is not configured in decal." + exc.Message);
            }
        }
        public static bool IsThwargFilterEnabled()
        {
            string subKey = @"SOFTWARE\Decal\NetworkFilters\{5C60E9C9-6F53-40EB-B2BE-5E67D76414B9}";
            try
            {
                RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(subKey);
                if (sk1 == null) { return false; }
                var ThwargFilterEnabled = (int)sk1.GetValue("Enabled", 0);
                if ((ThwargFilterEnabled != 1)) { return false; }

                return true;
            }
            catch (Exception exc)
            {
                throw new Exception("ThwargFilter is not enabled in decal." + exc.Message);
            }
        }
    }
}
