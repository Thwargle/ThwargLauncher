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
        public static bool IsMagfilterRegistered()
        {
            string subKey = @"SOFTWARE\Decal\NetworkFilters\{15395EC4-585E-4C59-9813-9C9F1654026C}";
            try
            {
                RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(subKey);
                if (sk1 == null) { return false; }
                string magfilterDLL = (string)sk1.GetValue("Path", "");
                if (string.IsNullOrEmpty(magfilterDLL)) { return false; }
                magfilterDLL += @"\MagFilter.dll";

                if (!File.Exists(magfilterDLL)) { return false; }

                return true;
            }
            catch (Exception exc)
            {
                throw new Exception("MagFilter is not configured in decal." + exc.Message);
            }
        }
        public static bool IsMagfilterEnabled()
        {
            string subKey = @"SOFTWARE\Decal\NetworkFilters\{15395EC4-585E-4C59-9813-9C9F1654026C}";
            try
            {
                RegistryKey sk1 = Registry.LocalMachine.OpenSubKey(subKey);
                if (sk1 == null) { return false; }
                var magfilterEnabled = (int)sk1.GetValue("Enabled", 0);
                if ((magfilterEnabled != 1)) { return false; }

                return true;
            }
            catch (Exception exc)
            {
                throw new Exception("MagFilter is not enabled in decal." + exc.Message);
            }
        }
    }
}
