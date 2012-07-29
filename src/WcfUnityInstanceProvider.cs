using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

namespace AylsCreations.Wcf
{
    /// <summary>
    /// The code is taken from here: http://initializecomponent.blogspot.com/2008/06/integrating-unity-with-wcf.html
    /// </summary>
    public class WcfUnityInstanceProvider : IInstanceProvider
    {
        public IUnityContainer Container { set; get; }
        public Type ServiceType { set; get; }

        public WcfUnityInstanceProvider()
            : this(null)
        {
        }

        public WcfUnityInstanceProvider(Type type)
        {
            ServiceType = type;
            Container = new UnityContainer();
        }

        #region IInstanceProvider Members

        public object GetInstance(
          InstanceContext instanceContext, Message message)
        {
            return Container.Resolve(ServiceType);
        }

        public object GetInstance(
          InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }
        public void ReleaseInstance(
          InstanceContext instanceContext, object instance)
        {
        }
        #endregion
    }
}