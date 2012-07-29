using System.ServiceModel;

namespace Ayls.DynamicWcfProxy.Sample.Contracts
{
    [ServiceContract]
    public interface IMyService
    {
        [OperationContract]
        string GetData(string name, int age);
    }
}
