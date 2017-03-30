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
        public event ReportSomethingDelegateMethod ReportingEvent;

        private Thread _thread = null;
        private IList<Server.ServerItem> _items;
        private int index;
        public void StartMonitor(IList<Server.ServerItem> items)
        {
            _thread = new Thread(new ThreadStart(MonitorLoop));
            _items = items;
            //_thread = new Thread(() => MonitorForever());
            _thread.Start();
        }
        public void StopMonitor()
        {
            _thread.Abort();
        }
        private void MonitorLoop()
        {
            Random random = new Random();
            int i = 0;
            while (true)
            {
                Thread.Sleep(5000);
                ++index;
                if (index >= _items.Count)
                {
                    index = 0;
                }
                var serverItem = _items[index];
                string status = string.Format("Random status #{0}", random.Next(40));
                CallToUpdate(serverItem, status);
            }
            
        }
        private void CallToUpdate(Server.ServerItem item, string status)
        {
            if (System.Windows.Application.Current == null) return;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal, new Action(() =>
                    {
                        PerformUpdate(item, status);
                    }));
        }
        /// <summary>
        /// Called on UI thread
        /// </summary>
        private void PerformUpdate(Server.ServerItem item, string status)
        {
            if (item.ConnectionStatus != status)
            {
                item.ConnectionStatus = status;
            }

        }
    }
}
