using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Configuration;
using System.Globalization;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPool
    {
        public readonly static ProxyPool Current = new ProxyPool();

        private readonly Hashtable _proxies;
        private int _maxNumberOfProxiesPerService;
        private int _closeConnectionOffset;

        private ProxyPool()
        {
            InitializeParameters();
            _proxies = new Hashtable();
        }

        private void InitializeParameters()
        {
            // Read common appSettings first...
            var configSection = (NameValueCollection)(ConfigurationManager.GetSection("dynamicWcfProxyPool"));
            if (configSection != null)
            {
                _maxNumberOfProxiesPerService = int.Parse(configSection["maxNumberOfProxiesPerService"]);
                _closeConnectionOffset = int.Parse(configSection["maxConnectionIdleTimeInSeconds"]) * 1000;
            } 
            else
            {
                _maxNumberOfProxiesPerService = 0; // infinite
                _closeConnectionOffset = 0; // this plus sendTimeout defines when the connections will be closed (offset defaults to 0 seconds)
            }
        }


        public ProxyPoolMember<T> GetProxy<T>() where T:class
        {
            Type t = typeof(T);
            lock (t)
            {

                List<ProxyPoolMember<T>> proxyCollection = null;
                ProxyPoolMember<T> cachedProxy = null;
                if (_proxies.ContainsKey(t)) 
                {
                    proxyCollection = (List<ProxyPoolMember<T>>)_proxies[t];

                    foreach (ProxyPoolMember<T> cp in proxyCollection)
                    {
                        if (!cp.InUse)
                        {
                            cachedProxy = cp;
                            break;
                        }
                    }
                }

                // no available proxy, create a new one
                if (cachedProxy == null)
                {
                    if (proxyCollection == null) 
                    {
                        proxyCollection = new List<ProxyPoolMember<T>>();
                        _proxies.Add(t, proxyCollection);
                    }

                    // if _maxNumberOfProxiesPerService is set to 0 we do not limit number of proxies in the pool
                    if (_maxNumberOfProxiesPerService > 0 && proxyCollection.Count >= _maxNumberOfProxiesPerService) 
                    {
                        string msg = String.Format("Pool is already at maximum capacity ({0}).", _maxNumberOfProxiesPerService.ToString(CultureInfo.InvariantCulture));
                        System.Diagnostics.Debug.WriteLine(msg);
                        throw new ProxyException(msg);
                    }

                    cachedProxy = new ProxyPoolMember<T>(_closeConnectionOffset);
                    proxyCollection.Add(cachedProxy);

                    System.Diagnostics.Debug.WriteLine(
                        String.Format("Created proxy {0} for {1}. Pool now contains {2} proxies.", cachedProxy.GetHashCode(), t.Name, proxyCollection.Count.ToString(CultureInfo.InvariantCulture)));

                }

                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} taken out of pool.", cachedProxy.GetHashCode(), t.Name));

                // mark as used
                cachedProxy.InUse = true;

                return cachedProxy;
            }
        }

        public void ReturnProxyToPool<T>(ProxyPoolMember<T> cachedProxy) where T:class
        {
            Type t = typeof(T);
            lock (t)
            {
                // mark as not used
                cachedProxy.InUse = false;

                System.Diagnostics.Debug.WriteLine(
                    String.Format("Proxy {0} for {1} returned to pool.", cachedProxy.GetHashCode(), t.Name));

            }
        }

    }
}
