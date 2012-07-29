using System;
using System.Globalization;
using System.Threading;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPoolMember<T> where T:class 
    {
        private readonly ChannelFactory<T> _serviceFactory;
        private readonly object _connectionLock = new object();
        private readonly int _proxyConnectionDuration;
        private Timer _timer;
        private DateTime _lastUsed;
        private T _proxy;

        public bool InUse { get; set; }

        public T Proxy
        {
            get
            {
                lock (_connectionLock)
                {
                    PrepareAndOpen();
                    return _proxy;
                }
            }
        }

        private Type GetProxyType() 
        {
            return typeof(T);
        }

        private ICommunicationObject GetCommunicationObject()
        {
            return (ICommunicationObject)_proxy;
        }

        public ProxyPoolMember(int closeConnectionOffset)
        {
            _serviceFactory = new ChannelFactory<T>(GetProxyType().Name + "Endpoint");
            _proxy = _serviceFactory.CreateChannel();
            // take send timeout (operation timeout) and add closeConnectionOffset on top of it to get the idle time after which connection should be closed
            // this is to avoid closing the connection before it timeouts which could lead to confusing error messages
            _proxyConnectionDuration = Convert.ToInt32(_serviceFactory.Endpoint.Binding.SendTimeout.TotalMilliseconds) + closeConnectionOffset;
        }

        private void PrepareAndOpen()
        {

            ICommunicationObject communicationObject = GetCommunicationObject();
            switch (communicationObject.State)
            {
                case CommunicationState.Closed:
                case CommunicationState.Closing:
                    _proxy = _serviceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject();
                    break;
                case CommunicationState.Faulted:
                    communicationObject.Abort();
                    _proxy = _serviceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject();
                    break;
            }

            if (communicationObject.State == CommunicationState.Created)
            {
                communicationObject.Open();
                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} opened.", GetHashCode().ToString(CultureInfo.InvariantCulture), GetProxyType().Name));
            }

            _lastUsed = DateTime.Now;
            StartConnectionCheck();

        }

        private void StartConnectionCheck()
        {
            if (_timer == null)
                _timer = new Timer(ConnectionCheck, null, _proxyConnectionDuration, _proxyConnectionDuration);
            else
                _timer.Change(_proxyConnectionDuration, _proxyConnectionDuration);
        }

        private void ConnectionCheck(object state)
        {
            lock (_connectionLock)
            {
                DateTime checkAt = DateTime.Now;
                if ((checkAt - _lastUsed).TotalMilliseconds >= _proxyConnectionDuration) 
                {
                    Close();
                    StopConnectionCheck();
                }
            }
        }

        private void StopConnectionCheck()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Close()
        {
            try
            {
                ICommunicationObject communicationObject = GetCommunicationObject();
                if (communicationObject.State == CommunicationState.Faulted) 
                    communicationObject.Abort();
                else if (communicationObject.State != CommunicationState.Closed && communicationObject.State != CommunicationState.Closed)
                    communicationObject.Close();

                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} closed.", GetHashCode().ToString(CultureInfo.InvariantCulture), GetProxyType().Name));
            }
            catch (Exception ex)
            {
                // not much that can be done here
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #region IDisposable implementation

        private bool _disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!_disposed) 
            {
                if (disposing)
                {
                    lock (_connectionLock)
                    {
                        Close();
                        StopConnectionCheck();
                    }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
