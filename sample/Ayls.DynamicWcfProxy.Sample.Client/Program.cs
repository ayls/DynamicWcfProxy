using System;

namespace Ayls.DynamicWcfProxy.Sample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // make sure to wrap the call into a using statement or else the proxy will not be returned to pool
            using (var proxy = new MyServiceProxy())
            {
                Console.WriteLine(proxy.GetData("Dude", 34));
            }

            Console.ReadLine();

            using (var proxy = new MyServiceProxy())
            {
                Console.WriteLine(proxy.GetData("Mate", 45));
            }

            Console.ReadLine();
        }
    }
}
