using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MagFilter
{
    public class LaunchControl
    {
        public class LaunchInfo
        {
            public bool IsValid;
            public DateTime LaunchTime;
            public string ServerName;
            public string AccountName;
            public string CharacterName;
        }
        public class LaunchResponse
        {
            public bool IsValid;
            public DateTime ResponseTime;
            public int ProcessId;
        }
        /// <summary>
        /// Called by Mag-Filter
        /// </summary>
        public LaunchInfo GetLaunchInfo()
        {
            var info = new LaunchInfo();
            string filepath = FileLocations.GetCurrentLaunchFilePath();

            if (!File.Exists(filepath)) { return info; }
            using (var file = new StreamReader(filepath))
            {
                string contents = file.ReadToEnd();
                string[] stringSeps = new string[] { "\r\n" };
                string[] lines = contents.Split(stringSeps, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length != 4) { return info; }
                if (!BeginsWith(lines[0], "TimeUtc:")
                    || !BeginsWith(lines[1], "ServerName:")
                    || !BeginsWith(lines[2], "AccountName:")
                    || !BeginsWith(lines[3], "CharacterName:")
                    )
                {
                    return info;
                }
                DateTime parsedTime;
                if (!DateTime.TryParse(lines[0].Substring("TimeUtc:".Length), out parsedTime))
                {
                    return info;
                }
                info.LaunchTime = parsedTime;
                TimeSpan maxLatency = new TimeSpan(0, 0, 5, 0);
                if (DateTime.UtcNow - info.LaunchTime >= maxLatency)
                {
                    return info;
                }
                info.ServerName = lines[1].Substring("ServerName:".Length);
                info.AccountName = lines[2].Substring("AccountName:".Length);
                info.CharacterName = lines[3].Substring("CharacterName:".Length);
                info.IsValid = true;
                return info;
            }
        }
        /// <summary>
        /// Called by Mag-Filter
        /// </summary>
        public LaunchResponse GetLaunchResponse(TimeSpan maxLatency)
        {
            var info = new LaunchResponse();
            string filepath = FileLocations.GetCurrentLaunchResponseFilePath();

            if (!File.Exists(filepath)) { return info; }
            using (var file = new StreamReader(filepath))
            {
                string contents = file.ReadToEnd();
                string[] stringSeps = new string[] { "\r\n" };
                string[] lines = contents.Split(stringSeps, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length != 2) { return info; }
                if (!BeginsWith(lines[0], "TimeUtc:")
                    || !BeginsWith(lines[1], "ProcessId:")
                    )
                {
                    return info;
                }
                DateTime parsedTime;
                if (!DateTime.TryParse(lines[3].Substring("TimeUtc:".Length), out parsedTime))
                {
                    return info;
                }
                info.ResponseTime = parsedTime;
                if (DateTime.UtcNow - info.ResponseTime >= maxLatency)
                {
                    return info;
                }
                string text = lines[0].Substring("ProcessId".Length);
                int parsedPid = 0;
                if (!int.TryParse(text, out parsedPid))
                {
                    return info;
                }
                info.ProcessId = parsedPid;
                info.IsValid = true;
            }
            return info;
        }
        /// <summary>
        /// Line starts with specified prefix and has at least one character beyond it
        ///  (primarily used to Substring(prefix.Length) will not fail
        /// </summary>
        private bool BeginsWith(string line, string prefix)
        {
            return line != null && line.StartsWith(prefix) && line.Length > prefix.Length;
        }
        /// <summary>
        /// Called by ThwargLauncher
        /// </summary>
        public void RecordLaunchInfo(string serverName, string accountName, string characterName, DateTime timestampUtc)
        {
            string filepath = FileLocations.GetCurrentLaunchFilePath();
            using (var file = new StreamWriter(filepath, append: false))
            {
                file.WriteLine("TimeUtc:" + timestampUtc);
                file.WriteLine("ServerName:" + serverName);
                file.WriteLine("AccountName:" + accountName);
                file.WriteLine("CharacterName:" + characterName);
            }
        }
        /// <summary>
        /// Called by Mag-Filter dll to pass pid to ThwargLauncher.exe
        /// </summary>
        public void RecordLaunchResponse(DateTime timestampUtc)
        {
            string filepath = FileLocations.GetCurrentLaunchResponseFilePath();
            using (var file = new StreamWriter(filepath, append: false))
            {
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                file.WriteLine("TimeUtc:" + timestampUtc);
                file.WriteLine("ProcessId:{0}", pid);
            }
        }
    }
}
