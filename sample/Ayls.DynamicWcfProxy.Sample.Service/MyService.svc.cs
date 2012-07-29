using System;
using Ayls.DynamicWcfProxy.Sample.Contracts;

namespace Ayls.DynamicWcfProxy.Sample.Service
{
    public class MyService : IMyService
    {
        public string GetData(string name, int age)
        {
            return string.Format("Hello {0}, your age is {1}", name, age);
        }
    }
}
