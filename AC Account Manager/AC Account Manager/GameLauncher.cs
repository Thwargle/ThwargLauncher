using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MagFilter;

namespace AC_Account_Manager
{
    class GameLauncher
    {
        public bool LaunchGameClient(string exelocation, string serverName, string accountName, string password, string desiredCharacter)
        {
            //-username "MyUsername" -password "MyPassword" -w "ServerName" -2 -3
            if (string.IsNullOrWhiteSpace(exelocation)) { throw new Exception("Empty exelocation"); }
            if (string.IsNullOrWhiteSpace(serverName)) { throw new Exception("Empty serverName"); }
            if (string.IsNullOrWhiteSpace(accountName)) { throw new Exception("Empty accountName"); }
            if (string.IsNullOrWhiteSpace(password)) { throw new Exception("Empty password"); }
            string arg1 = accountName;
            string arg2 = password;
            string arg3 = serverName;

            string genArgs = "-username " + arg1 + " -password " + arg2 + " -w " + arg3 + " -2 -3";
            string pathToFile = exelocation;
            if (arg2 == "")
            {
                // TODO - currently not supported
                genArgs = "-username " + arg1 + " -w " + arg3 + " -3 ";
            }
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pathToFile;
                startInfo.Arguments = genArgs;
                startInfo.CreateNoWindow = true;

                RecordLaunchInfo(serverName, accountName, desiredCharacter);

                // This is analogous to Process.Start or CreateProcess
                Process gameProc = Process.Start(startInfo);
                do
                {
                    while (!gameProc.HasExited)
                    {
                        gameProc.Refresh();
                        if (gameProc.Responding)
                        {
                            return true;
                        }
                    }
                } while (!gameProc.WaitForExit(1000));
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format(
                    "Failed to launch program. Check path '{0}': {1}",
                    exelocation, exc.Message));
            }
            return true;
        }
        private void RecordLaunchInfo(string serverName, string accountName, string desiredCharacter)
        {
            var ctl = new LaunchControl();
            ctl.RecordLaunchInfo(serverName: serverName, accountName: accountName, characterName: desiredCharacter);
        }
    }
}
