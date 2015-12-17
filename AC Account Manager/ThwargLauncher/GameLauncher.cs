using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using MagFilter;

namespace ThwargLauncher
{
    public delegate bool ShouldStopLaunching(object sender, EventArgs e);
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
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pathToFile;
                startInfo.Arguments = genArgs;
                startInfo.CreateNoWindow = true;

                RecordLaunchInfo(serverName, accountName, desiredCharacter, DateTime.UtcNow);

                // This is analogous to Process.Start or CreateProcess
                string charFilepath = MagFilter.FileLocations.GetCharacterFilePath();
                DateTime startWait = DateTime.UtcNow;
                DateTime loginTime = DateTime.MaxValue;
                using (FileSystemWatcher fw = WatchFile(charFilepath))
                {
                    Process launcherProc = Process.Start(startInfo);
                    bool gameReady = false;
                    fw.Changed += delegate(object sender, FileSystemEventArgs args) { gameReady = true; };
                    if (!gameReady)
                    {
                        WaitForLauncher(launcherProc);
                        int secondsTimeout = ConfigSettings.GetConfigInt("LauncherGameTimeoutSeconds", 120);
                        TimeSpan timeout = new TimeSpan(0, 0, 0, secondsTimeout);
                        while (!gameReady && (DateTime.UtcNow - startWait < timeout))
                        {
                            if (CheckForStop())
                                return false;
                            System.Threading.Thread.Sleep(1000);
                            FileInfo fileInfo = new FileInfo(charFilepath);
                            if (loginTime == DateTime.MaxValue)
                            {
                                // First we wait until DLL writes character file
                                if (fileInfo.LastWriteTime.ToUniversalTime() >= startWait)
                                {
                                    loginTime = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                // Then we give it 6 more seconds to complete login
                                if (DateTime.UtcNow >= loginTime.AddSeconds(6))
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
        private void RecordLaunchInfo(string serverName, string accountName, string desiredCharacter, DateTime timestamp)
        {
            var ctl = new LaunchControl();
            ctl.RecordLaunchInfo(serverName: serverName, accountName: accountName, characterName: desiredCharacter,
                                 timestamp: timestamp);
        }
    }
}
