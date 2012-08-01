using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    public class ProxyContext<T> where T : class 
    {
        public static ChannelFactory<T> ServiceFactory = new ChannelFactory<T>(typeof(T).Name + "Endpoint");

        public int MaxNumberOfProxiesPerService { get; private set; }

        public string EndpointName { get; set; }

        public ProxyConnectionLifeCycleStrategyBase<T> ConnectionLifeCycleStrategy { get; private set; }

        public ProxyContext() : this(new DefaultProxyConnectionLifeCycleStrategy<T>())
        {
        }

        public ProxyContext(ProxyConnectionLifeCycleStrategyBase<T> connectionStrategy)
        {
            // TODO limit to one connection strategy per endpoint
            ConnectionLifeCycleStrategy = connectionStrategy;

            // limit the maximum number of connections as per configuration
            if (ServiceFactory.Endpoint.Binding is NetTcpBinding)
                MaxNumberOfProxiesPerService = ((NetTcpBinding)ServiceFactory.Endpoint.Binding).MaxConnections;
            else if (ServiceFactory.Endpoint.Binding is NetNamedPipeBinding)
                MaxNumberOfProxiesPerService = ((NetNamedPipeBinding)ServiceFactory.Endpoint.Binding).MaxConnections;
            else
                MaxNumberOfProxiesPerService = int.MaxValue;
        }
    }
}
