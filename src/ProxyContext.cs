using System;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    class ProxyContext<T> where T : class 
    {
        public static ChannelFactory<T> ServiceFactory = new ChannelFactory<T>(typeof(T).Name + "Endpoint");

        public int MaxConnectionIdleTime { get; private set; }

        public int MaxNumberOfProxiesPerService { get; private set; }

        public ProxyContext()
        {
            // take send timeout (operation timeout) and add 5 seconds on top of it to get the idle time after which connection should be closed
            // this is to avoid closing the connection before it timeouts which could lead to confusing error messages
            MaxConnectionIdleTime = Convert.ToInt32(ServiceFactory.Endpoint.Binding.SendTimeout.TotalMilliseconds) + 5000;

            // limit the maximum number of connections as per configuration
            if (ServiceFactory.Endpoint.Binding is NetTcpBinding)
                MaxNumberOfProxiesPerService = ((NetTcpBinding) ServiceFactory.Endpoint.Binding).MaxConnections;
            else if (ServiceFactory.Endpoint.Binding is NetNamedPipeBinding)
                MaxNumberOfProxiesPerService = ((NetNamedPipeBinding) ServiceFactory.Endpoint.Binding).MaxConnections;
            else
                MaxNumberOfProxiesPerService = int.MaxValue;
        }
    }
}
