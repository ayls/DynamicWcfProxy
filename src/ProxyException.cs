using System;

namespace Ayls.DynamicWcfProxy
{
    public class ProxyException : Exception
    {
        public ProxyException()
            : base()
        {
        }

        public ProxyException(string msg) : base(msg)
        {
        }
    }
}
