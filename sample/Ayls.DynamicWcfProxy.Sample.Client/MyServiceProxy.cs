using Ayls.DynamicWcfProxy.Sample.Contracts;

namespace Ayls.DynamicWcfProxy.Sample.Client
{
    class MyServiceProxy : ProxyBase<IMyService>
    {
        public string GetData(string name, int age)
        {
            return ExecuteProxyFunction(() => Proxy.GetData(name, age));
        }
    }
}
