using System;
using System.Globalization;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    public abstract class ProxyConnectionLifeCycleStrategyBase<T> where T : class 
    {
        protected T Proxy;

        protected ProxyConnectionLifeCycleStrategyBase()
        {
            Proxy = ProxyContext<T>.ServiceFactory.CreateChannel();
        }

        public virtual T Open()
        {
            var proxyOut = Proxy;

            ICommunicationObject communicationObject = GetCommunicationObject(Proxy);
            switch (communicationObject.State)
            {
                case CommunicationState.Closed:
                case CommunicationState.Closing:
                    proxyOut = ProxyContext<T>.ServiceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject(Proxy);
                    break;
                case CommunicationState.Faulted:
                    communicationObject.Abort();
                    proxyOut = ProxyContext<T>.ServiceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject(Proxy);
                    break;
            }

            if (communicationObject.State == CommunicationState.Created)
            {
                communicationObject.Open();
                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} opened.", 
                    Proxy.GetHashCode().ToString(CultureInfo.InvariantCulture),
                    ProxyContext<T>.ServiceFactory.Endpoint.Name));
            }

            return proxyOut;
        }

        public virtual void Close()
        {
            try
            {
                ICommunicationObject communicationObject = GetCommunicationObject(Proxy);
                if (communicationObject.State == CommunicationState.Faulted)
                    communicationObject.Abort();
                else if (communicationObject.State != CommunicationState.Closed && communicationObject.State != CommunicationState.Closed)
                    communicationObject.Close();

                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} closed.", 
                    Proxy.GetHashCode().ToString(CultureInfo.InvariantCulture),
                    ProxyContext<T>.ServiceFactory.Endpoint.Name));
            }
            catch (Exception ex)
            {
                // not much that can be done here
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected ICommunicationObject GetCommunicationObject(T proxy)
        {
            return (ICommunicationObject)proxy;
        }
    }
}
