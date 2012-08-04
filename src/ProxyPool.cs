using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPool
    {
        public readonly static ProxyPool Current = new ProxyPool();

        private Dictionary<string, object> _endpointContextKeys; 
        private readonly Hashtable _endpointContexts; 
        
        private ProxyPool()
        {
            _endpointContextKeys = new Dictionary<string, object>();
            _endpointContexts = new Hashtable();
        }

        private EndpointKey<T> GetEndpointKey<T>(string endpointName) where T : class
        {
            Type t = typeof (T);
            lock (t)
            {
                if (_endpointContextKeys.ContainsKey(endpointName))
                {
                    return (EndpointKey<T>)_endpointContextKeys[endpointName];
                }
                else
                {
                    var key = new EndpointKey<T>(endpointName);
                    _endpointContextKeys.Add(endpointName, key);
                    return key;
                }
            }
        }

        public ProxyPoolMember<T> GetProxy<T>(string endpointName, ConnectionLifeCycleStrategyBase<T> connectionLifeCycleStrategy) where T:class
        {
            var key = GetEndpointKey<T>(endpointName);
            lock (key)
            {
                EndpointContext<T> endpointContext = null;
                ProxyPoolMember<T> cachedProxy = null;
                if (_endpointContexts.ContainsKey(key)) 
                {
                    endpointContext = (EndpointContext<T>)_endpointContexts[key];

                    foreach (ProxyPoolMember<T> cp in endpointContext.Proxies)
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
                    if (endpointContext == null)
                    {
                        endpointContext = new EndpointContext<T>(endpointName, connectionLifeCycleStrategy);
                        connectionLifeCycleStrategy.EndpointContext = endpointContext;
                        _endpointContexts.Add(key, endpointContext);
                    }

                    if (!endpointContext.CanAddAnotherProxy)
                    {
                        string msg = String.Format("Pool for {0} is already at maximum capacity ({1}).", key.ToString(), endpointContext.MaxNumberOfProxiesPerEndpoint.ToString(CultureInfo.InvariantCulture));
                        System.Diagnostics.Debug.WriteLine(msg);
                        throw new ProxyException(msg);
                    }

                    cachedProxy = new ProxyPoolMember<T>(endpointContext);
                    endpointContext.Proxies.Add(cachedProxy);

                    System.Diagnostics.Debug.WriteLine(
                        String.Format("Created proxy {0} for {1}. Pool now contains {2} proxies.", cachedProxy.GetHashCode(), key.ToString(), endpointContext.Proxies.Count.ToString(CultureInfo.InvariantCulture)));

                }

                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} taken out of pool.", cachedProxy.GetHashCode(), key.ToString()));

                // mark as used
                cachedProxy.InUse = true;

                return cachedProxy;
            }
        }

        public void ReturnProxy<T>(ProxyPoolMember<T> cachedProxy) where T : class
        {
            var key = GetEndpointKey<T>(cachedProxy.EndpointContext.EndpointName);
            lock (key)
            {
                // mark as not used
                cachedProxy.InUse = false;

                System.Diagnostics.Debug.WriteLine(
                    String.Format("Proxy {0} for {1} returned to pool.", cachedProxy.GetHashCode(), key.ToString()));

            }
        }

    }
}
