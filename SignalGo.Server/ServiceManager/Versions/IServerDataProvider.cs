using SignalGo.Server.Models;
using SignalGo.Shared.IO;
using System;
using System.Net.Sockets;

namespace SignalGo.Server.ServiceManager.Versions
{
    public interface IServerDataProvider : IDisposable
    {
        void Start(ServerBase serverBase, int port);
        ClientInfo CreateClientInfo(ServerBase serverBase, ClientInfo client, TcpClient tcpClient, PipeNetworkStream stream);
    }
}
