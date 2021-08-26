using SignalGo.Server.Models;
using SignalGo.Shared.Log;
using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Firewall
{
    internal class FirewallBase
    {
        AutoLogger Logger { get; set; } = new AutoLogger() { FileName = "Firewall.log" };
        internal IFirewall DefaultFirewall { get; set; }
        public Task<object> OnCallingMethod(ClientInfo clientInfo, string serviceName, Type serviceType, string methodName, MethodInfo methodInfo, object[] parameters)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnCallingMethod(clientInfo, serviceName, serviceType, methodName, methodInfo, parameters);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnCallingMethod");
                }
            }

            return Task.FromResult<object>(null);
        }

        public Task<bool> OnClientInitialized(ClientInfo clientInfo)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnClientInitialized(clientInfo);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnClientInitialized");
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> OnDangerDataReceived(TcpClient tcpClient, DangerDataType dangerDataType)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnDangerDataReceived(tcpClient, dangerDataType);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnDangerDataReceived");
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> OnServerInternalError(ClientInfo clientInfo, Exception exception)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnServerInternalError(clientInfo, exception);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnServerInternalError");
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> OnTcpClientConnected(TcpClient tcpClient)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnTcpClientConnected(tcpClient);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnTcpClientConnected");
                }
            }

            return Task.FromResult(true);
        }
    }
}
