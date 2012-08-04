using System.Collections.Generic;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    public interface IEndpointContext<T> where T : class
    {
        string EndpointName { get; }
        ChannelFactory<T> ServiceFactory { get; }
        List<IProxyPoolMember<T>> Proxies { get; }
        ConnectionLifeCycleStrategyBase<T> ConnectionLifeCycleStrategy { get; }

        int MaxNumberOfProxiesPerEndpoint { get; }

        bool CanAddAnotherProxy { get; }
    }
}
