using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

namespace AylsCreations.Wcf
{
    /// <summary>
    /// The code is taken from here: http://initializecomponent.blogspot.com/2008/06/integrating-unity-with-wcf.html
    /// </summary>
    public class WcfUnityServiceBehavior : IServiceBehavior
    {
        public WcfUnityInstanceProvider InstanceProvider
        { get; set; }

        private ServiceHost serviceHost = null;

        public WcfUnityServiceBehavior()
        {
            InstanceProvider = new WcfUnityInstanceProvider();
        }
        public WcfUnityServiceBehavior(IUnityContainer unity)
        {
            InstanceProvider = new WcfUnityInstanceProvider();
            InstanceProvider.Container = unity;
        }
        public void ApplyDispatchBehavior(
          ServiceDescription serviceDescription,
          ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase cdb
                 in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher cd
                   = cdb as ChannelDispatcher;
                if (cd != null)
                {
                    foreach (EndpointDispatcher ed
                                    in cd.Endpoints)
                    {
                        InstanceProvider.ServiceType
                             = serviceDescription.ServiceType;
                        ed.DispatchRuntime.InstanceProvider
                             = InstanceProvider;

                    }
                }
            }
        }
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters) { }

        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase) { }

        public void AddToHost(ServiceHost host)
        {
            // only add to host once
            if (serviceHost != null) return;
            host.Description.Behaviors.Add(this);
            serviceHost = host;
        }
    }
}