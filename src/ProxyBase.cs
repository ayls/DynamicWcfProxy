using System;

namespace Ayls.DynamicWcfProxy
{
    public class ProxyBase<T> : IDisposable
        where T : class
    {
        private readonly ProxyPoolMember<T> _pooledProxy;
        private readonly ProxyContext<T> _context; 

        public ProxyBase()
        {
            _context = new ProxyContext<T>();
            _pooledProxy = ProxyPool.Current.GetProxy<T>(_context);
        }

        public T Proxy
        {
            get
            {
                return _pooledProxy.Proxy;
            }
        }

        public TResult ExecuteProxyFunction<TResult>(Func<TResult> f)
        {
            return f.Invoke();
        }

        public void ExecuteProxyMethod(Action f)
        {
            f.Invoke();
        }

        #region IDisposable implementation

        private bool _disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    if (_pooledProxy != null)
                        ProxyPool.Current.ReturnProxy(_pooledProxy, _context);
            }
            _disposed = true;
        }

        #endregion
    }
}
