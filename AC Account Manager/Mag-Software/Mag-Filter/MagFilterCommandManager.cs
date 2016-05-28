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
        private const string CMDTest = "/mf test ";
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
                    string commandString = e.Text.Substring(CMDCommand.Length, e.Text.Length - CMDCommand.Length);
                    Heartbeat.SendCommand(commandString);
                    Heartbeat.SendAndReceiveImmediately();
                }
                e.Eat = true;
            }
            else if (e.Text.StartsWith(CMDTest))
            {
                if (e.Text.Length > CMDTest.Length)
                {
                    string commandString = e.Text.Substring(CMDTest.Length, e.Text.Length - CMDTest.Length);
                    Mag.Shared.PostMessageTools.SendMsg(commandString);
                }
                e.Eat = true;
            }
        }
    }
}
