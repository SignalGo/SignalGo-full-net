using SignalGo.Server.Models;
using SignalGo.Shared.Log;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Firewall
{
    internal class FirewallBase
    {
        AutoLogger Logger { get; set; } = new AutoLogger() { FileName = "Firewall.log" };
        internal IFirewall DefaultFirewall { get; set; }
        public Task<object> OnCallingMethod(ClientInfo clientInfo, string serviceName, string methodName, SignalGo.Shared.Models.ParameterInfo[] parameters, string jsonParameters)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnCallingMethod(clientInfo, serviceName, methodName, parameters, jsonParameters);
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

        public Task<bool> OnDangerDataReceived(TcpClient tcpClient, ClientInfo clientInfo, DangerDataType dangerDataType)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnDangerDataReceived(tcpClient, clientInfo, dangerDataType);
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

        public bool OnHttpHeaderComepleted(TcpClient tcpClient, ref string key, ref string value)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnHttpHeaderComepleted(tcpClient, ref key, ref value);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnHttpHeaderComepleted");
                }
            }

            return true;
        }

        public Task<bool> OnHttpHeadersComepleted(ClientInfo clientInfo)
        {
            if (DefaultFirewall != null)
            {
                try
                {
                    return DefaultFirewall.OnHttpHeadersComepleted(clientInfo);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "OnHttpHeadersComepleted");
                }
            }

            return Task.FromResult(true);
        }
    }
}
