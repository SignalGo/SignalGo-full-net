using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    public class ServerDataProviderV4 : IServerDataProvider
    {
        TcpListener _server;
        ServerBase _serverBase;
#if (NET35 || NET40)
        public void Start(ServerBase serverBase, int port)
#else
        public async void Start(ServerBase serverBase, int port)
#endif
        {
            _serverBase = serverBase;
            Exception exception = null;
            try
            {
                _server = new TcpListener(IPAddress.IPv6Any, port);
#if (NET35)
#else
                _server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
#endif
                _server.Server.NoDelay = true;

                _server.Start();
                if (serverBase.ProviderSetting.IsEnabledToUseTimeout)
                {
                    _server.Server.SendTimeout = (int)serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
                    _server.Server.ReceiveTimeout = (int)serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                }
                serverBase.IsStarted = true;
                while (true)
                {
                    try
                    {
#if (NET35 || NET40)
                        var client = _server.AcceptTcpClient();
#else
                        var client = await _server.AcceptTcpClientAsync();
#endif
                        InitializeClient(client);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
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

        /// <summary>
        /// initialzie and read client
        /// </summary>
        /// <param name="tcpClient"></param>
#if (NET35 || NET40)
        public void InitializeClient(TcpClient tcpClient)
#else
        public async void InitializeClient(TcpClient tcpClient)
#endif
        {
#if (NET35 || NET40)
            Task.Factory.StartNew(() =>
#else
            await Task.Run(async () =>
#endif
            {
                try
                {
                    OperationContext.CurrentTaskServer = _serverBase;
#if (NET35 || NET40)
                    var stream = ReadFirstLineOfClient(tcpClient);
#else
                    var stream = await ReadFirstLineOfClient(tcpClient);
#endif
                    ExchangeClient(stream, stream.Line, stream.LastBytesReaded, tcpClient);
                }
                catch (Exception)
                {
#if (NETSTANDARD)
                    tcpClient.Dispose();

#else
                    tcpClient.Close();
#endif
                }
            });
        }

        /// <summary>
        /// read first line of client data
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="result"></param>
        /// <returns></returns>
#if (NET35 || NET40)
        public CustomStreamReader ReadFirstLineOfClient(TcpClient tcpClient)
#else
        public async Task<CustomStreamReader> ReadFirstLineOfClient(TcpClient tcpClient)
#endif
        {
            var reader = new CustomStreamReader(tcpClient.GetStream());

#if (NET35 || NET40)
            var data = reader.ReadLine();
#else
            var data =await reader.ReadLine();
#endif
            return reader;
        }
        /// <summary>
        /// create client information
        /// </summary>
        /// <param name="isHttp"></param>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public ClientInfo CreateClientInfo(bool isHttp, TcpClient tcpClient)
        {
            ClientInfo client = null;
            if (isHttp)
                client = new HttpClientInfo();
            else
                client = new ClientInfo();
            client.ConnectedDateTime = DateTime.Now;
            client.TcpClient = tcpClient;
            client.IPAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Replace("::ffff:", "");
            client.ClientId = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
            _serverBase.Clients.TryAdd(client.ClientId, client);
            client.ClientStream = tcpClient.GetTcpStream(_serverBase);
            return client;
        }

        /// <summary>
        /// Exchange data from client and server 
        /// </summary>
        /// <param name="firstLineString"></param>
        /// <param name="firstLineBytes"></param>
        /// <param name="tcpClient"></param>
        public void ExchangeClient(CustomStreamReader reader, string firstLineString, byte[] firstLineBytes, TcpClient tcpClient)
        {
            //File.WriteAllBytes("I:\\signalgotext.txt", reader.LastBytesReaded);
            ClientInfo client = null;
            try
            {
                if (firstLineString.Contains("SignalGo-Stream/4.0"))
                {
                    client = CreateClientInfo(false, tcpClient);
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    SignalGoStreamProvider.StartToReadingClientData(client, _serverBase);
                }
                else if (firstLineString.Contains("SignalGo-OneWay/4.0"))
                {
                    client = CreateClientInfo(false, tcpClient);
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    OneWayServiceProvider.StartToReadingClientData(client, _serverBase);
                }
                else if (firstLineString.Contains("SignalGo/4.0"))
                {
                    client = CreateClientInfo(false, tcpClient);
                    //"SignalGo/1.0";
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;

                    SignalGoDuplexServiceProvider.StartToReadingClientData(client, _serverBase);
                }
                else if (firstLineString.Contains("HTTP/"))
                {
                    HttpProvider.StartToReadingClientData(tcpClient, _serverBase, reader, firstLineString);
                }
                else
                {
                    _serverBase.DisposeClient(client, "AddClient header not support");
                }
            }
            catch (Exception)
            {
                _serverBase.DisposeClient(client, "exception");
            }
        }
    }
}
