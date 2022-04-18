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
        Task<bool> OnTcpClientConnected(TcpClient tcpClient);
        /// <summary>
        /// when client initialized 
        /// </summary>
        /// <param name="clientInfo"></param>
        Task<bool> OnClientInitialized(ClientInfo clientInfo);

        /// <summary>
        /// 1 MB is your Danger maximum received data from client
        /// danger data is like 1 MB size of first line in http
        /// or 1 MB header size
        /// or 1 MB request size
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="clientInfo"></param>
        /// <param name="dangerDataType"></param>
        /// <returns></returns>
        Task<bool> OnDangerDataReceived(TcpClient tcpClient, ClientInfo clientInfo, DangerDataType dangerDataType);
        /// <summary>
        /// when server had unhandled internal or external error
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task<bool> OnServerInternalError(ClientInfo clientInfo, Exception exception);
        /// <summary>
        /// when any http header read comeplete
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool OnHttpHeaderComepleted(TcpClient tcpClient, ref string key, ref string value);
        /// <summary>
        /// when all of headers readed completed
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        Task<bool> OnHttpHeadersComepleted(ClientInfo clientInfo);
        /// <summary>
        /// when client want to call your server method
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="serviceName"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="jsonParameters"></param>
        /// <returns>if you set the result this will return to client before do anything</returns>
        Task<object> OnCallingMethod(ClientInfo clientInfo, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters, string jsonParameters);
    }
}
