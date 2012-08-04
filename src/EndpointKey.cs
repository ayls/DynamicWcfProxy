using System;

namespace Ayls.DynamicWcfProxy
{
    class EndpointKey<T> where T : class 
    {
        public EndpointKey(string endpointName)
        {
            EndpointName = endpointName;
        }

        public string EndpointName { get; private set; }

        public override bool Equals(object obj)
        {
            var otherKey = (EndpointKey<T>)obj;
            if (otherKey == null)
                return false;
            else
                return otherKey.EndpointName == EndpointName;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", typeof(T).FullName, EndpointName);
        }
    }
}
