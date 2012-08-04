namespace Ayls.DynamicWcfProxy
{
    public interface IProxyPoolMember<T> where T : class
    {
        bool InUse { get; set; }

        T Proxy { get; }
    }
}
