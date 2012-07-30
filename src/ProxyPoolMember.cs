using System;
using System.Globalization;
using System.Threading;
using System.ServiceModel;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPoolMember<T> where T:class
    {
        private readonly ProxyContext<T> _context; 
        private readonly object _connectionLock = new object();
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

        public ProxyPoolMember(ProxyContext<T> context)
        {
            _context = context;
            _proxy = ProxyContext<T>.ServiceFactory.CreateChannel();
        }

        private void PrepareAndOpen()
        {

            ICommunicationObject communicationObject = GetCommunicationObject();
            switch (communicationObject.State)
            {
                case CommunicationState.Closed:
                case CommunicationState.Closing:
                    _proxy = ProxyContext<T>.ServiceFactory.CreateChannel();
                    communicationObject = GetCommunicationObject();
                    break;
                case CommunicationState.Faulted:
                    communicationObject.Abort();
                    _proxy = ProxyContext<T>.ServiceFactory.CreateChannel();
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
                _timer = new Timer(ConnectionCheck, null, _context.MaxConnectionIdleTime, _context.MaxConnectionIdleTime);
            else
                _timer.Change(_context.MaxConnectionIdleTime, _context.MaxConnectionIdleTime);
        }

        private void ConnectionCheck(object state)
        {
            lock (_connectionLock)
            {
                DateTime checkAt = DateTime.Now;
                if ((checkAt - _lastUsed).TotalMilliseconds >= _context.MaxConnectionIdleTime) 
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
