using System;
using System.Globalization;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    public abstract class ConnectionLifeCycleStrategyBase<T> where T : class
    {
        private T _proxy;
        protected T Proxy
        {
            get
            {
                if (_proxy == null)
                    _proxy = EndpointContext.ServiceFactory.CreateChannel();

                return _proxy;
            }
        }

        public IEndpointContext<T> EndpointContext { get; set; }

        public virtual T Open()
        {
            var proxyOut = Proxy;

            ICommunicationObject communicationObject = GetCommunicationObject(Proxy);
            switch (communicationObject.State)
            {
                case CommunicationState.Closed:
                case CommunicationState.Closing:
                    proxyOut = EndpointContext.ServiceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject(Proxy);
                    break;
                case CommunicationState.Faulted:
                    communicationObject.Abort();
                    proxyOut = EndpointContext.ServiceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject(Proxy);
                    break;
            }

            if (communicationObject.State == CommunicationState.Created)
            {
                communicationObject.Open();
                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} ({2}) opened.", 
                    Proxy.GetHashCode().ToString(CultureInfo.InvariantCulture),
                    typeof(T).FullName,
                    EndpointContext.ServiceFactory.Endpoint.Name));
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

                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} ({2}) closed.", 
                    Proxy.GetHashCode().ToString(CultureInfo.InvariantCulture),
                    typeof(T).FullName,
                    EndpointContext.ServiceFactory.Endpoint.Name));
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
