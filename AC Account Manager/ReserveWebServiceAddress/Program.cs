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
            uint port = 33000;
            foreach (string arg in args)
            {
                if (arg.StartsWith("user:", StringComparison.CurrentCultureIgnoreCase))
                {
                    accountName = arg.Substring("user:".Length);
                }
                else if (arg.StartsWith("port:", StringComparison.CurrentCultureIgnoreCase))
                {
                    accountName = arg.Substring("port:".Length);
                }
                else
                {
                    Console.WriteLine("The only options recognized are user:dom\\user and port:999");
                    return;
                }
            }
            string netshArgs = string.Format(
                "http add urlacl url=http://+:{0}/ user={1}",
                port,
                accountName);
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", netshArgs);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            Process.Start(procStartInfo);
        }
    }
}
