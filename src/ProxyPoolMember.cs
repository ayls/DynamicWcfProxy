using System;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPoolMember<T> : IProxyPoolMember<T> where T: class
    {
        public readonly EndpointContext<T> EndpointContext; 

        public bool InUse { get; set; }

        public T Proxy
        {
            get
            {
                return EndpointContext.ConnectionLifeCycleStrategy.Open();
            }
        }

        public ProxyPoolMember(EndpointContext<T> endpointContext)
        {
            EndpointContext = endpointContext;
        }

        #region IDisposable implementation

        private bool _disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!_disposed) 
            {
                if (disposing)
                {
                    EndpointContext.ConnectionLifeCycleStrategy.Close();
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
