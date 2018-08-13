using SignalGo.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    public interface IServerDataProvider
    {
        void Start(ServerBase serverBase, int port);
        ClientInfo CreateClientInfo(bool isHttp, TcpClient tcpClient);
    }
}
