using System;
using Ayls.DynamicWcfProxy.Sample.Contracts;

namespace Ayls.DynamicWcfProxy.Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // make sure to wrap the call into a using statement or else the proxy will not be returned to pool
            using (var client = new ProxyBase<IMyService>())
            {
                Console.WriteLine(client.ExecuteProxyFunction(() => client.Proxy.GetData("Dude", 34)));
            }

            Console.ReadLine();

            using (var client = new ProxyBase<IMyService>())
            {
                Console.WriteLine(client.ExecuteProxyFunction(() => client.Proxy.GetData("Joe", 45)));
            }

            Console.ReadLine();
        }
    }
}
