using System;
using System.ServiceModel;
using Microsoft.Practices.Unity;

namespace AylsCreations.Wcf
{
    public class WcfUnityServiceHost : ServiceHost
    {
        public IUnityContainer Container { set; get; }

        public WcfUnityServiceHost()
            : base()
        {
            Container = new UnityContainer();
        }

        public WcfUnityServiceHost(
            Type serviceType,
            params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            Container = new UnityContainer();
        }

        protected override void OnOpening()
        {
            new WcfUnityServiceBehavior(Container)
                       .AddToHost(this);
            base.OnOpening();
        }
    }
}