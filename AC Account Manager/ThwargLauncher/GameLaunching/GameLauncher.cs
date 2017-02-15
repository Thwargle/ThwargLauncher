using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using MagFilter;
using System.Runtime.InteropServices;

namespace ThwargLauncher
{
    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = (0x0001),
        SUSPEND_RESUME = (0x0002),
        GET_CONTEXT = (0x0008),
        SET_CONTEXT = (0x0010),
        SET_INFORMATION = (0x0020),
        QUERY_INFORMATION = (0x0040),
        SET_THREAD_TOKEN = (0x0080),
        IMPERSONATE = (0x0100),
        DIRECT_IMPERSONATION = (0x0200)
    }
    public class GameLaunchResult
    {
        public bool Success;
        public int ProcessId;
    }
    public delegate bool ShouldStopLaunching(object sender, EventArgs e);
    /// <summary>
    /// Called by Launch Manager to actually fire off a process for one game
    /// Called on worker thread
    /// </summary>
    class GameLauncher
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public event ShouldStopLaunching StopLaunchEvent;

        public delegate void ReportGameStatusHandler(string status);
        public event ReportGameStatusHandler ReportGameStatusEvent;
        private void ReportGameStatus(string status)
        {
            if (ReportGameStatusEvent != null)
            {
                ReportGameStatusEvent(status);
            }
        }

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

        public GameLaunchResult LaunchGameClient(string exelocation, string serverName, string accountName, string password, string ipAddress, string desiredCharacter)
        {
            var result = new GameLaunchResult();
            //-username "MyUsername" -password "MyPassword" -w "ServerName" -2 -3
            if (string.IsNullOrWhiteSpace(exelocation)) { throw new Exception("Empty exelocation"); }
            if (string.IsNullOrWhiteSpace(serverName)) { throw new Exception("Empty serverName"); }
            if (string.IsNullOrWhiteSpace(accountName)) { throw new Exception("Empty accountName"); }
            string arg1 = accountName;
            string arg2 = password;

            string genArgs = "TODO-below";

            bool isPhat = true;
            if (isPhat)
            {
                //PHATAC
                //-h [server ip] -p [server port] -a username:password -rodat off
                int tok = ipAddress.IndexOf(':');
                string ip = ipAddress.Substring(0, tok);
                string port = ipAddress.Substring(tok + 1);
                string genArgsPhatServer = "-h " + ip + " -p " + port + " -a " + arg1 + ":" + arg2 + " -rodat off";
                genArgs = genArgsPhatServer;

            }
            else
            {
                //ACE
                //acclient.exe -a testaccount -h 127.0.0.1:9000 -glsticketdirect testpassword
                string genArgsACEServer = "-a " + accountName + " -h " + ipAddress + " -glsticketdirect " + arg2;
                genArgs = genArgsACEServer;

            }

            string pathToFile = exelocation;
            bool gameReady = false;
            Process launcherProc = null;
            LaunchControl.LaunchResponse launchResponse = null;
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

                startInfo.WorkingDirectory = Path.GetDirectoryName(startInfo.FileName);
                launcherProc = Process.Start(startInfo);
                //SuspendProcess(launcherProc.Id);
                //UtilityCode.BasicInject.InjectDecal(launcherProc);
                //ResumeProcess(launcherProc.Id);
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
                            return result;
                                
                        }
                        ReportGameStatus(string.Format("Waiting for game: {0}/{1} sec",
                            (int)((DateTime.UtcNow - startWait).TotalSeconds), secondsTimeout));
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
                        else if (loginTime == DateTime.MaxValue)
                        {
                            // Now we wait until DLL logs in or user logs in interactively
                            FileInfo fileInfo = new FileInfo(launchResponseFilepath);
                            if (fileInfo.LastWriteTime.ToUniversalTime() >= startWait)
                            {
                                loginTime = DateTime.UtcNow;
                                TimeSpan maxLatency = DateTime.UtcNow - startWait;
                                launchResponse = LaunchControl.GetLaunchResponse(maxLatency);
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
            if (launchResponse != null && launchResponse.IsValid)
            {
                result.Success = gameReady;
                result.ProcessId = launchResponse.ProcessId;
            }
            return result;
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
            DateTime startUtc = DateTime.UtcNow;
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
                ReportGameStatus(string.Format("Waiting for launcher: {0} seconds",
                    (int)((DateTime.UtcNow - startUtc).TotalSeconds)));
            } while (!launcherProc.WaitForExit(1000));
        }
        private void RecordLaunchInfo(string serverName, string accountName, string desiredCharacter, DateTime timestampUtc)
        {
            LaunchControl.RecordLaunchInfo(serverName: serverName, accountName: accountName, characterName: desiredCharacter,
                                 timestampUtc: timestampUtc);

            // TODO
            var x = LaunchControl.DebugGetLaunchInfo();
            // verify x
        }
        private static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }
    }
}