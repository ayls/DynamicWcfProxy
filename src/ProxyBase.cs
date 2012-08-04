using System;

namespace Ayls.DynamicWcfProxy
{
    public class ProxyBase<T> : IDisposable
        where T : class
    {
        private readonly ProxyPoolMember<T> _pooledProxy;

        public ProxyBase() : this(new DefaultConnectionLifeCycleStrategy<T>())
        {
        }

        public ProxyBase(ConnectionLifeCycleStrategyBase<T> connectionLifeCycleStrategy) : this(typeof(T).Name + "Endpoint", connectionLifeCycleStrategy)
        {
        }

        public ProxyBase(string endpointName, ConnectionLifeCycleStrategyBase<T> connectionLifeCycleStrategy)
        {
            _pooledProxy = ProxyPool.Current.GetProxy<T>(endpointName, connectionLifeCycleStrategy);
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
