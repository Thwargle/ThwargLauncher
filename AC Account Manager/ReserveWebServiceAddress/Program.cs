using System;
using System.Text;
using System.Diagnostics;

namespace ReserveWebServiceAddress
{
    class Program
    {
        static void Main(string[] args)
        {
            string accountName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string netshArgs = string.Format(
                "http add urlacl url=http://+:33000/ user={0}",
                accountName);
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", netshArgs);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            Process.Start(procStartInfo);
        }
    }
}
