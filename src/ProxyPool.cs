using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;

namespace Ayls.DynamicWcfProxy
{
    class ProxyPool
    {
        struct HashTableKey
        {
            public HashTableKey(Type type, string endpointName)
            {
                _type = type;
                _endpointName = endpointName;
            }

            readonly Type _type;
            readonly string _endpointName;

            public override bool Equals(object obj)
            {
                var otherKey = (HashTableKey)obj;
                return otherKey._type == _type && otherKey._endpointName == _endpointName;
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("{0} ({1})", _type.FullName, _endpointName);
            }
        }

        public readonly static ProxyPool Current = new ProxyPool();

        private readonly Hashtable _proxies;
        
        private ProxyPool()
        {
            _proxies = new Hashtable();
        }

        private HashTableKey GetHashTableKey(Type type, string endpointName)
        {
            return new HashTableKey(type, endpointName);
        }

        public ProxyPoolMember<T> GetProxy<T>(ProxyContext<T> context) where T:class
        {
            // TODO lock down locks to key level
            Type t = typeof(T);
            lock (t)
            {
                var key = GetHashTableKey(t, context.EndpointName);

                List<ProxyPoolMember<T>> proxyCollection = null;
                ProxyPoolMember<T> cachedProxy = null;
                if (_proxies.ContainsKey(key)) 
                {
                    proxyCollection = (List<ProxyPoolMember<T>>)_proxies[key];

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
                        _proxies.Add(key, proxyCollection);
                    }

                    if (context.MaxNumberOfProxiesPerService < int.MaxValue && proxyCollection.Count >= context.MaxNumberOfProxiesPerService) 
                    {
                        string msg = String.Format("Pool for {0} is already at maximum capacity ({1}).", key.ToString(), context.MaxNumberOfProxiesPerService.ToString(CultureInfo.InvariantCulture));
                        System.Diagnostics.Debug.WriteLine(msg);
                        throw new ProxyException(msg);
                    }

                    cachedProxy = new ProxyPoolMember<T>(context);
                    proxyCollection.Add(cachedProxy);

                    System.Diagnostics.Debug.WriteLine(
                        String.Format("Created proxy {0} for {1}. Pool now contains {2} proxies.", cachedProxy.GetHashCode(), key.ToString(), proxyCollection.Count.ToString(CultureInfo.InvariantCulture)));

                }

                System.Diagnostics.Debug.WriteLine(String.Format("Proxy {0} for {1} taken out of pool.", cachedProxy.GetHashCode(), key.ToString()));

                // mark as used
                cachedProxy.InUse = true;

                return cachedProxy;
            }
        }

        public void ReturnProxy<T>(ProxyPoolMember<T> cachedProxy, ProxyContext<T> context) where T : class
        {
            Type t = typeof(T);
            lock (t)
            {
                var key = GetHashTableKey(t, context.EndpointName);

                // mark as not used
                cachedProxy.InUse = false;

                System.Diagnostics.Debug.WriteLine(
                    String.Format("Proxy {0} for {1} returned to pool.", cachedProxy.GetHashCode(), key.ToString()));

            }
        }

    }
}
