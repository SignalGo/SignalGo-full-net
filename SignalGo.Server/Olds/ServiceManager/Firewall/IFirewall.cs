using SignalGo.Server.Models;
using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Firewall
{
    /// <summary>
    /// signal go firewall interfarce
    /// </summary>
    public interface IFirewall
    {
        /// <summary>
        /// when tcp client connected to server
        /// this is base of tcp connected to your server
        /// </summary>
        /// <param name="tcpClient"></param>
        Task OnTcpClientConnected(TcpClient tcpClient);
        /// <summary>
        /// when client initialized 
        /// </summary>
        /// <param name="clientInfo"></param>
        Task OnClientInitialized(ClientInfo clientInfo);
        /// <summary>
        /// when server had unhandled internal or external error
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task OnServerInternalError(ClientInfo clientInfo, Exception exception);
        /// <summary>
        /// when client want to call your server method
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="serviceName"></param>
        /// <param name="serviceType"></param>
        /// <param name="methodName"></param>
        /// <param name="methodInfo"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task OnCallingMethod(ClientInfo clientInfo, string serviceName, Type serviceType, string methodName, MethodInfo methodInfo, object[] parameters);
    }
}
