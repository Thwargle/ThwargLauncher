using System;
using System.Collections.Generic;

using Decal.Adapter;
using Mag.Shared;

namespace MagFilter
{
    class MagFilterCommandManager
    {
        private const string CMDVersion = "/mf version";
        private const string CMDCommand = "/mf command ";
        public void FilterCore_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            if (e.Text.StartsWith(CMDVersion))
            {

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string msg = string.Format(
                    "MagFilter, AssemblyVer: {0}, AssemblyFileVer: {1}",
                    assembly.GetName().Version,
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)
                                    );
                
                Debug.WriteToChat("Version: " + msg);
                log.WriteLogMsg("Called Debug.WriteToChat Version: " + msg);

                e.Eat = true;
            }
            else if (e.Text.StartsWith(CMDCommand))
            {
                if (e.Text.Length > CMDCommand.Length)
                {
                    string command = e.Text.Substring(CMDCommand.Length, e.Text.Length - CMDCommand.Length);
                    Heartbeat.SendImmediateMessage("Command", command);
                }
                e.Eat = true;
            }
        }
    }
}
