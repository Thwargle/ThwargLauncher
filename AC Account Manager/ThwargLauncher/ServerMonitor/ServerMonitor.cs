using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ThwargLauncher
{
    class ServerMonitor
    {
        public delegate void ReportSomethingDelegateMethod(string msg);

        private Thread _thread = null;
        private IList<Server.ServerItem> _items;
        private int _secondsDelay = 5 * 60;
        const int TIMEOUTSEC = 3;
        public void StartMonitor(IList<Server.ServerItem> items)
        {
            StopMonitor();
            _thread = new Thread(new ThreadStart(MonitorLoop));
            _items = items;
            _thread.Start();
        }
        public void StopMonitor()
        {
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }
        }
        private async void MonitorLoop()
        {
            Random random = new Random();
            while (true)
            {
                await CheckAllServers();
                Thread.Sleep(TimeSpan.FromSeconds(_secondsDelay));
            }
        }
        private async Task CheckAllServers()
        {
            await Task.WhenAll(_items.Select(s => CheckServer(s)).ToArray());
        }
        private async Task CheckServer(Server.ServerItem server)
        {
            var address = AddressParser.Parse(server.ServerIpAndPort);
            if (string.IsNullOrEmpty(address.Ip) || address.Port <= 0) { return; }
            bool up = await IsUdpServerUp(address.Ip, address.Port);
            string status = GetStatusString(up);
            if (server.ConnectionStatus != status)
            {
                CallToUpdate(server, status);
            }
        }
        private async Task<bool> IsUdpServerUp(string address, int port)
        {
            try
            {
                UdpClient udpClient = new UdpClient();
                // udpClient.Client.ReceiveTimeout not used in Async calls
                udpClient.Connect(address, port);
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] sendBytes = Packet.MakeLoginPacket();
                //Byte[] sendBytes = ConstructPacket();
                udpClient.Send(sendBytes, sendBytes.Length);
                var receiveTask = udpClient.ReceiveAsync();
                var tsk = await Task.WhenAny(receiveTask, Task.Delay(TimeSpan.FromSeconds(TIMEOUTSEC)));
                if (tsk == receiveTask)
                {
                    return true;
                }
                else
                {
                    // TODO: clean up udpClient?
                    return false;
                }
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10054)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
        private byte[] ConstructPacket()
        {
            var data = new Packet.PacketHeader(Packet.PacketHeaderFlags.EchoRequest);
            uint checksum;
            data.CalculateHash32(out checksum);
            data.Checksum = checksum;
            return data.GetRaw();
        }
        private static string GetStatusString(bool up)
        {
            return (up ? "Online" : "Offline");
        }
        private bool IsTcpServerUp(string address, int port)
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
