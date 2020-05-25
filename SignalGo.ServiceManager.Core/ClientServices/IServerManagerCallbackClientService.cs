using SignalGo.Shared.DataTypes;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.ClientServices
{
    [ServiceContract("ServerManagerCallback", ServiceType.ClientService, InstanceType.SingleInstance)]
    public interface IServerManagerCallbackClientService : IServerManagerCallbackClientServiceBase
    {
        Task ReceivedMessageAsync(string message);
    }
}
