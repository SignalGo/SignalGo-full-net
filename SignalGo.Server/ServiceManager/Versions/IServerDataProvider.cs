using SignalGo.Server.Models;
using SignalGo.Shared.IO;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    public abstract class ServerDataProviderBase
    {
        public Action<ServerBase, int> StartAction { get; set; }
        public Func<ServerBase, PipeNetworkStream, TcpClient, Task> ExchangeClientFunc { get; set; }
        public Func<ServerBase, ClientInfo, TcpClient, PipeNetworkStream, ClientInfo> CreateClientFunc { get; set; }
    }
}
