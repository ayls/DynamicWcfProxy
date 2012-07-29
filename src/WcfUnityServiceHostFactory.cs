using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace AylsCreations.Wcf
{
    public class WcfUnityServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(
                                          Type serviceType, Uri[] baseAddresses)
        {
            WcfUnityServiceHost host = new WcfUnityServiceHost(serviceType, baseAddresses);
            IUnityContainer container =  new UnityContainer();
            container.LoadConfiguration();
            host.Container = container;

            return host;
        }
    }
}