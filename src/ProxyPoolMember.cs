using System;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPoolMember<T> where T:class
    {
        private readonly ProxyConnectionLifeCycleStrategyBase<T> _connectionStrategy; 

        public bool InUse { get; set; }

        public T Proxy
        {
            get
            {
                return _connectionStrategy.Open();
            }
        }

        public ProxyPoolMember(ProxyContext<T> context)
        {
            _connectionStrategy = context.ConnectionLifeCycleStrategy;
        }

        #region IDisposable implementation

        private bool _disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!_disposed) 
            {
                if (disposing)
                {
                    _connectionStrategy.Close();
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
