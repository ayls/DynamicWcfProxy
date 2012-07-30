using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPool
    {
        public readonly static ProxyPool Current = new ProxyPool();

        private readonly Hashtable _proxies;

        private ProxyPool()
        {
            _proxies = new Hashtable();
        }

        public ProxyPoolMember<T> GetProxy<T>(ProxyContext<T> context) where T:class
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

                    if (context.MaxNumberOfProxiesPerService < int.MaxValue && proxyCollection.Count >= context.MaxNumberOfProxiesPerService) 
                    {
                        string msg = String.Format("Pool is already at maximum capacity ({0}).", context.MaxNumberOfProxiesPerService.ToString(CultureInfo.InvariantCulture));
                        System.Diagnostics.Debug.WriteLine(msg);
                        throw new ProxyException(msg);
                    }

                    cachedProxy = new ProxyPoolMember<T>(context);
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

        public void ReturnProxy<T>(ProxyPoolMember<T> cachedProxy) where T:class
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
