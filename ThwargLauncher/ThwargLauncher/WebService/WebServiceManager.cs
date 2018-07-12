using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace ThwargLauncher.WebService
{
    class WebServiceManager
    {
        private ServiceHost _serviceHost = null;
        public void Listen()
        {
            var baseAddress = new Uri("http://localhost:33000/ThwargLauncher/WebService/ThwargListener");
            _serviceHost = new ServiceHost(typeof(ThwargListenerHandler), baseAddress);
            _serviceHost.AddServiceEndpoint(
                typeof (IThwargListener),
                new WSHttpBinding(),
                "ThwargService");

            // Enable metadata exchange
            var smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            _serviceHost.Description.Behaviors.Add(smb);

            _serviceHost.Open();
        }
        public void StopListening()
        {
            if (_serviceHost != null && _serviceHost.State == CommunicationState.Opened)
            {
                _serviceHost.Close();
                
            }
        }
    }
}
