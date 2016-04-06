using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ThwargLauncher
{
    /// <summary>
    /// Called on worker thread
    /// </summary>
    class LaunchManager
    {
        public delegate void ReportStatusHandler(string status, LaunchSorter.LaunchItem launchItem);
        public event ReportStatusHandler ReportStatusEvent;

        readonly Dictionary<string, DateTime> _accountLaunchTimes = new Dictionary<string, DateTime>();
        private string _launcherLocation;

        public LaunchManager(string launcherLocation)
        {
            _launcherLocation = launcherLocation;

        }
        public bool LaunchGameHandlingDelaysAndTitles(BackgroundWorker worker, LaunchSorter.LaunchItem launchItem)
        {
            if (worker.CancellationPending)
            {
                return false;
            }
            DateTime lastLaunch = (_accountLaunchTimes.ContainsKey(launchItem.AccountName)
                                       ? _accountLaunchTimes[launchItem.AccountName]
                                       : DateTime.MinValue);
            TimeSpan delay = new TimeSpan(0, 5, 0) - (DateTime.Now - lastLaunch);
            while (delay.TotalMilliseconds > 0)
            {
                if (worker.CancellationPending)
                {
                    return false;
                }
                string context = string.Format("Waiting {0} sec", (int)delay.TotalSeconds + 1);
                ReportStatus(context, launchItem);

                System.Threading.Thread.Sleep(1000);
                delay = new TimeSpan(0, 0, 10) - (DateTime.Now - lastLaunch);
            }

            ReportStatus("Launching", launchItem);
            _accountLaunchTimes[launchItem.AccountName] = DateTime.Now;

            var launcher = new GameLauncher();
            launcher.StopLaunchEvent += (o, eventArgs) => { return worker.CancellationPending; };
            try
            {
                var finder = new ThwargUtils.WindowFinder();
                finder.RecordExistingWindows();
                string launcherPath = GetLaunchItemLauncherLocation(launchItem);
                OverridePreferenceFile(launchItem.CustomPreferencePath);
                bool okgo = launcher.LaunchGameClient(
                    launcherPath,
                    launchItem.ServerName,
                    accountName: launchItem.AccountName,
                    password: launchItem.Password,
                    desiredCharacter: launchItem.CharacterSelected
                    );
                if (!okgo)
                {
                    return false;
                }
                string gameCaptionPattern = ConfigSettings.GetConfigString("GameCaptionPattern", null);
                if (gameCaptionPattern != null)
                {
                    var regex = new System.Text.RegularExpressions.Regex(gameCaptionPattern);
                    IntPtr hwnd = finder.FindNewWindow(regex);
                    if (hwnd != IntPtr.Zero)
                    {
                        string newGameTitle = GetNewGameTitle(launchItem);
                        if (!string.IsNullOrEmpty(newGameTitle))
                        {
                            finder.SetWindowTitle(hwnd, newGameTitle);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                ReportStatus("Exception launching game launcher: " + exc.Message, launchItem);
                return false;
            }
            return true;
        }
        private void OverridePreferenceFile(string customPreferencePath)
        {
            // Non-customizing launches need to restore active copy from base
            if (string.IsNullOrEmpty(customPreferencePath))
            {
                if (File.Exists(Configuration.UserPreferencesBaseFile))
                {
                    File.Copy(Configuration.UserPreferencesBaseFile, Configuration.UserPreferencesFile, overwrite: true);
                }
                return;
            }
            // customizing launches:
            if (!File.Exists(customPreferencePath)) { return; }
            // Backup actual file first

            if (!File.Exists(Configuration.UserPreferencesBaseFile))
            {
                File.Copy(Configuration.UserPreferencesFile, Configuration.UserPreferencesBaseFile, overwrite: false);
                if (!File.Exists(Configuration.UserPreferencesBaseFile)) { return; }
            }
            // Now overwrite
            File.Copy(customPreferencePath, Configuration.UserPreferencesFile, overwrite: true);
        }

        private string GetLaunchItemLauncherLocation(LaunchSorter.LaunchItem item)
        {
            if (!string.IsNullOrEmpty(item.CustomLaunchPath))
            {
                return item.CustomLaunchPath;
            }
            else
            {
                return _launcherLocation;
            }
        }
        private string GetNewGameTitle(LaunchSorter.LaunchItem launchItem)
        {
            if (launchItem.CharacterSelected == "None")
            {
                string pattern = ConfigSettings.GetConfigString("NewGameTitleNoChar", "");
                pattern = pattern.Replace("%ACCOUNT%", launchItem.AccountName);
                pattern = pattern.Replace("%SERVER%", launchItem.ServerName);
                return pattern;

            }
            else
            {
                string pattern = ConfigSettings.GetConfigString("NewGameTitle", "");
                pattern = pattern.Replace("%ACCOUNT%", launchItem.AccountName);
                pattern = pattern.Replace("%SERVER%", launchItem.ServerName);
                pattern = pattern.Replace("%CHARACTER%", launchItem.CharacterSelected);
                return pattern;
            }
        }
        private void ReportStatus(string status, LaunchSorter.LaunchItem launchItem)
        {
            if (ReportStatusEvent != null)
            {
                ReportStatusEvent(status, launchItem);
            }
        }

    }
}
