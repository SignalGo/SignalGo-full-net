using System.Threading.Tasks;

namespace SignalGo.ServiceManager.ClientServices
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServerManagerCallbackClientServiceBase
    {
        Task ReceivedMessageBaseAsync(string message);
    }
}
