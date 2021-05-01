using SignalGo.Server.Models;
using SignalGo.Shared.IO;
using System.Net.Sockets;

namespace SignalGo.Server.ServiceManager.Versions
{
    public interface IServerDataProvider
    {
        bool CanAcceptClient { get; set; }
        void Start(ServerBase serverBase, int port);
        ClientInfo CreateClientInfo(bool isHttp, TcpClient tcpClient, PipeNetworkStream stream);
        string GetInformation();
        int GetConnectedCount();
    }
}
