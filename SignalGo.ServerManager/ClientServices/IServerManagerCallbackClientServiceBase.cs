using System.Threading.Tasks;

namespace SignalGo.ServerManager.ClientServices
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServerManagerCallbackClientServiceBase
    {
        Task ReceivedMessageBaseAsync(string message);
    }
}
