using System.Collections;
using System.Collections.Generic;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    class EndpointContext<T> : IEndpointContext<T> where T : class
    {
        public string EndpointName { get; private set; }
        public ChannelFactory<T> ServiceFactory { get; private set; }
        public List<IProxyPoolMember<T>> Proxies { get; private set; }
        public ConnectionLifeCycleStrategyBase<T> ConnectionLifeCycleStrategy { get; private set; }

        public int MaxNumberOfProxiesPerEndpoint { get; private set; }

        public bool CanAddAnotherProxy
        {
            get { return MaxNumberOfProxiesPerEndpoint == int.MinValue || MaxNumberOfProxiesPerEndpoint > Proxies.Count; }
        }

        public EndpointContext(string endpointName, ConnectionLifeCycleStrategyBase<T> connectionLifeCycleStrategy)
        {
            EndpointName = endpointName;
            ServiceFactory = new ChannelFactory<T>(EndpointName);
            Proxies = new List<IProxyPoolMember<T>>();
            ConnectionLifeCycleStrategy = connectionLifeCycleStrategy;

            // limit the maximum number of connections as per configuration
            if (ServiceFactory.Endpoint.Binding is NetTcpBinding)
                MaxNumberOfProxiesPerEndpoint = ((NetTcpBinding)ServiceFactory.Endpoint.Binding).MaxConnections;
            else if (ServiceFactory.Endpoint.Binding is NetNamedPipeBinding)
                MaxNumberOfProxiesPerEndpoint = ((NetNamedPipeBinding)ServiceFactory.Endpoint.Binding).MaxConnections;
            else
                MaxNumberOfProxiesPerEndpoint = int.MinValue;   
        }
    }
}
