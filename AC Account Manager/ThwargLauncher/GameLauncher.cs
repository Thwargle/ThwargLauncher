using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using MagFilter;

namespace ThwargLauncher
{
    public delegate bool ShouldStopLaunching(object sender, EventArgs e);
    /// <summary>
    /// Called by Launch Manager to actually fire off a process for one game
    /// Called on worker thread
    /// </summary>
    class GameLauncher
    {
        public event ShouldStopLaunching StopLaunchEvent;

        private bool CheckForStop()
        {
            if (StopLaunchEvent != null)
            {
                return StopLaunchEvent(this, null);
            }
            else
            {
                return false;
            }
        }

        public bool LaunchGameClient(string exelocation, string serverName, string accountName, string password, string desiredCharacter)
        {
            //-username "MyUsername" -password "MyPassword" -w "ServerName" -2 -3
            if (string.IsNullOrWhiteSpace(exelocation)) { throw new Exception("Empty exelocation"); }
            if (string.IsNullOrWhiteSpace(serverName)) { throw new Exception("Empty serverName"); }
            if (string.IsNullOrWhiteSpace(accountName)) { throw new Exception("Empty accountName"); }
            string arg1 = accountName;
            string arg2 = password;
            string arg3 = serverName;

            string genArgs = "-username " + arg1 + " -password " + arg2 + " -w " + arg3 + " -2 -3";
            string pathToFile = exelocation;
            if (arg2 == "")
            {
                genArgs = "-username " + arg1 + " -w " + arg3 + " -3 ";
            }
            bool gameReady = false;
            Process launcherProc = null;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pathToFile;
                startInfo.Arguments = genArgs;
                startInfo.CreateNoWindow = true;

                RecordLaunchInfo(serverName, accountName, desiredCharacter, DateTime.UtcNow);

                // This is analogous to Process.Start or CreateProcess
                string charFilepath = MagFilter.FileLocations.GetCharacterFilePath();
                string launchResponseFilepath = MagFilter.FileLocations.GetCurrentLaunchResponseFilePath();
                DateTime startWait = DateTime.UtcNow;
                DateTime characterFileWrittenTime = DateTime.MaxValue;
                DateTime loginTime = DateTime.MaxValue;
                using (FileSystemWatcher fw = WatchFile(charFilepath))
                {
                    launcherProc = Process.Start(startInfo);
                    //fw.Changed += delegate(object sender, FileSystemEventArgs args) { gameReady = true; };
                    if (!gameReady)
                    {
                        WaitForLauncher(launcherProc);
                        int secondsTimeout = ConfigSettings.GetConfigInt("LauncherGameTimeoutSeconds", 120);
                        TimeSpan timeout = new TimeSpan(0, 0, 0, secondsTimeout);
                        while (!gameReady && (DateTime.UtcNow - startWait < timeout))
                        {
                            if (CheckForStop())
                            {
                                // User canceled
                                if (!launcherProc.HasExited)
                                {
                                    launcherProc.Kill();
                                }
                                return false;
                                
                            }
                            System.Threading.Thread.Sleep(1000);
                            if (characterFileWrittenTime == DateTime.MaxValue)
                            {
                                // First we wait until DLL writes character file
                                FileInfo fileInfo = new FileInfo(charFilepath);
                                if (fileInfo.LastWriteTime.ToUniversalTime() >= startWait)
                                {
                                    characterFileWrittenTime = DateTime.UtcNow;
                                }
                            }
                            else if (IsValidCharacterName(desiredCharacter) && loginTime == DateTime.MaxValue)
                            {
                                // Now we wait until DLL logs in
                                FileInfo fileInfo = new FileInfo(launchResponseFilepath);
                                if (fileInfo.LastWriteTime.ToUniversalTime() >= startWait)
                                {
                                    loginTime = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                // Then we give it 6 more seconds to complete login
                                int loginTimeSeconds = ConfigSettings.GetConfigInt("LauncherGameLoginTime", 0);
                                if (DateTime.UtcNow >= characterFileWrittenTime.AddSeconds(loginTimeSeconds))
                                {
                                    gameReady = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format(
                    "Failed to launch program. Check path '{0}': {1}",
                    exelocation, exc.Message));
            }
            if (!gameReady)
            {
                if (launcherProc != null && !launcherProc.HasExited)
                {
                    launcherProc.Kill();
                }
            }
            return gameReady;
        }
        private bool IsValidCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) { return false; }
            if (characterName == "None") { return false; }
            return true;
        }
        private FileSystemWatcher WatchFile(string filepath)
        {
            return new FileSystemWatcher(
                Path.GetDirectoryName(filepath),
                Path.GetFileName(filepath)
                );
        }
        private void WaitForLauncher(Process launcherProc)
        {
            do
            {
                while (!launcherProc.HasExited)
                {
                    if(CheckForStop())
                        return;
                    launcherProc.Refresh();
                    if (launcherProc.Responding)
                    {
                        return;
                    }
                }
            } while (!launcherProc.WaitForExit(1000));
        }
        private void RecordLaunchInfo(string serverName, string accountName, string desiredCharacter, DateTime timestampUtc)
        {
            var ctl = new LaunchControl();
            ctl.RecordLaunchInfo(serverName: serverName, accountName: accountName, characterName: desiredCharacter,
                                 timestampUtc: timestampUtc);
        }
    }
}
