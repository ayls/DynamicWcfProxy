using System;

namespace Ayls.DynamicWcfProxy
{
    public class ProxyBase<T> : IDisposable
        where T : class
    {
        private readonly ProxyPoolMember<T> _pooledProxy = null;

        public ProxyBase()
        {
            _pooledProxy = ProxyPool.Current.GetProxy<T>(new ProxyContext<T>());
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
                        ProxyPool.Current.ReturnProxy(_pooledProxy);
            }
            _disposed = true;
        }

        #endregion
    }
}
