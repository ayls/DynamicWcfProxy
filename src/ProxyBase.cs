using System;

namespace Ayls.DynamicWcfProxy
{
    public abstract class ProxyBase<T> : IDisposable
        where T : class
    {
        private readonly ProxyPoolMember<T> _pooledProxy = null;

        protected ProxyBase()
        {
            _pooledProxy = ProxyPool.Current.GetProxy<T>();
        }

        public T Proxy
        {
            get
            {
                return _pooledProxy.Proxy;
            }
        }

        protected TResult ExecuteProxyFunction<TResult>(Func<TResult> f)
        {
            return f.Invoke();
        }

        protected void ExecuteProxyMethod(Action f)
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
                        ProxyPool.Current.ReturnProxyToPool(_pooledProxy);
            }
            _disposed = true;
        }

        #endregion
    }
}
