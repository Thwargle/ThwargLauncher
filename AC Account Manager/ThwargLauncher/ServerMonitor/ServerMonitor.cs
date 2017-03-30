using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace ThwargLauncher
{
    class ServerMonitor
    {
        public delegate void ReportSomethingDelegateMethod(string msg);

        private Thread _thread = null;
        private IList<Server.ServerItem> _items;
        private int _millisecondsDelay = 1000;
        private int index;
        public void StartMonitor(IList<Server.ServerItem> items)
        {
            _thread = new Thread(new ThreadStart(MonitorLoop));
            _items = items;
            _thread.Start();
        }
        public void StopMonitor()
        {
            _thread.Abort();
        }
        private void MonitorLoop()
        {
            Random random = new Random();
            while (true)
            {
                Thread.Sleep(_millisecondsDelay);
                ++index;
                if (index >= _items.Count)
                {
                    index = 0;
                    _millisecondsDelay = 5000;
                }
                var server = _items[index];
                CheckServer(server);
            }
        }
        private void CheckServer(Server.ServerItem server)
        {
            var address = AddressParser.Parse(server.ServerIpAndPort);
            if (string.IsNullOrEmpty(address.Ip) || address.Port <= 0) { return; }
            bool up = IsServerUp(address.Ip, address.Port);
            string status = GetStatusString(up);
            if (server.ConnectionStatus != status)
            {
                CallToUpdate(server, status);
            }
        }
        private static string GetStatusString(bool up)
        {
            return (up ? "Online" : "Offline");
        }
        private bool IsServerUp(string address, int port)
        {
            var tcpClient = new System.Net.Sockets.TcpClient();
            try
            {
                tcpClient.Connect(address, port);
                return true;
            }
            catch (Exception exc)
            {
                string debug = exc.ToString();
                return false;
            }
        }
        private void CallToUpdate(Server.ServerItem server, string status)
        {
            if (System.Windows.Application.Current == null) return;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal, new Action(() =>
                    {
                        PerformUpdate(server, status);
                    }));
        }
        /// <summary>
        /// Called on UI thread
        /// </summary>
        private void PerformUpdate(Server.ServerItem server, string status)
        {
            if (server.ConnectionStatus != status)
            {
                server.ConnectionStatus = status;
            }

        }
    }
}
