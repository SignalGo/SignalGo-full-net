using SignalGo.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    /// <summary>
    /// data provider engine for version 1 of signalgo
    /// </summary>
    public class ServerDataProviderV1
    {
        internal ConcurrentList<string> VirtualDirectories { get; set; } = new ConcurrentList<string>();
        TcpListener _server;
        internal async void Start(ServerBase serverBase, int port, string[] virtualUrl)
        {
            Exception exception = null;
            try
            {
                _server = new TcpListener(IPAddress.IPv6Any, port);
#if (NET35)
#else
                _server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
#endif
                _server.Server.NoDelay = true;

                foreach (var item in virtualUrl)
                {
                    if (!VirtualDirectories.Contains(item))
                        VirtualDirectories.Add(item);
                }

                _server.Start();
                if (serverBase.ProviderSetting.IsEnabledToUseTimeout)
                {
                    _server.Server.SendTimeout = (int)serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
                    _server.Server.ReceiveTimeout = (int)serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                }
                serverBase.IsStarted = true;
                while (true)
                {
                    var client = await _server.AcceptTcpClientAsync();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server Disposed! : " + ex);
                serverBase.OnServerInternalExceptionAction?.Invoke(ex);
                serverBase.AutoLogger.LogError(ex, "Connect Server");
                exception = ex;
                serverBase.Stop();
            }
            if (exception != null)
                throw exception;
        }
    }
}
