using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.ClientServices
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServerManagerCallbackClientServiceBase
    {
        Task ReceivedMessageBaseAsync(string message);
    }
}
