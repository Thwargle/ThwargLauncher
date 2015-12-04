using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using MagFilter;

namespace ThwargLauncher
{
    class GameLauncher
    {
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
                DateTime startWait = DateTime.Now;
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
                        while (!gameReady && (DateTime.Now - startWait < timeout))
                        {
                            System.Threading.Thread.Sleep(10000);
                            FileInfo fileInfo = new FileInfo(charFilepath);
                            if (fileInfo.LastWriteTime >= startWait)
                            {
                                gameReady = true;
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
