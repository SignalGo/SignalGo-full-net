using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.Converters;
using SignalGo.Shared.IO;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    public class ServerDataProviderV4 : IServerDataProvider
    {
        internal TcpListener _server;
        private ServerBase _serverBase;
        internal bool IsWaitForClient { get; set; }

        private readonly object _lockobject = new object();
        private volatile int _ConnectedCount;
        private volatile int _WaitingToReadFirstLineCount;
#if (NET35 || NET40)
        public void Start(ServerBase serverBase, int port)
#else
        public void Start(ServerBase serverBase, int port)
#endif
        {
            Thread thread = new Thread(() =>
            {
                _serverBase = serverBase;
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
                            //IsWaitForClient = true;
#if (NETSTANDARD1_6)
                            TcpClient client = _server.AcceptTcpClientAsync().GetAwaiter().GetResult();
#else
                            TcpClient client = _server.AcceptTcpClient();

#endif
                            _ConnectedCount++;
                            //IsWaitForClient = false;
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
                }
                finally
                {
                    Console.WriteLine("server finished");
                    serverBase.Stop();
                }
            })
            {
                IsBackground = false
            };
            thread.Start();
        }

        /// <summary>
        /// initialzie and read client
        /// </summary>
        /// <param name="tcpClient"></param>
        public void InitializeClient(TcpClient tcpClient)
        {
            Task.Run(async () =>
            {
                try
                {
                    tcpClient.GetStream().ReadTimeout = 5000;
                    tcpClient.GetStream().WriteTimeout = 5000;
                    PipeNetworkStream stream = new PipeNetworkStream(new NormalStream(await tcpClient.GetTcpStream(_serverBase)), (int)_serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds);
                    ExchangeClient(stream, tcpClient);
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
        /// create client information
        /// </summary>
        /// <param name="isHttp"></param>
        /// <param name="tcpClient"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public ClientInfo CreateClientInfo(bool isHttp, TcpClient tcpClient, PipeNetworkStream stream)
        {
            ClientInfo client = null;
            if (isHttp)
                client = new HttpClientInfo(_serverBase);
            else
                client = new ClientInfo(_serverBase);
            client.ConnectedDateTime = DateTime.Now;
            client.TcpClient = tcpClient;
            //client.IPAddressBytes = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Replace("::ffff:", "");
            client.IPAddressBytes = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.GetAddressBytes();
            client.ClientId = Guid.NewGuid().ToString();
            _serverBase.Clients.TryAdd(client.ClientId, client);
            client.ClientStream = stream;
            _serverBase.OnClientConnectedAction?.Invoke(client);
            return client;
        }

        /// <summary>
        /// Exchange data from client and server 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="tcpClient"></param>
        public async void ExchangeClient(PipeNetworkStream reader, TcpClient tcpClient)
        {
            //File.WriteAllBytes("I:\\signalgotext.txt", reader.LastBytesReaded);
            ClientInfo client = null;
            try
            {
                if (_serverBase.ProviderSetting.IsEnabledToUseTimeout)
                {
                    tcpClient.GetStream().ReadTimeout = (int)_serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                    tcpClient.GetStream().WriteTimeout = (int)_serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
                }

                string firstLineString = await reader.ReadLineAsync();
                if (firstLineString.Contains("SignalGo-Stream/4.0"))
                {
                    if (!_serverBase.ProviderSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = -1;
                        tcpClient.GetStream().WriteTimeout = -1;
                    }
                    client = CreateClientInfo(false, tcpClient, reader);
                    client.ProtocolType = ClientProtocolType.SignalGoStream;
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    SignalGoStreamProvider.StartToReadingClientData(client, _serverBase);
                }
                else if (firstLineString.Contains("SignalGo-OneWay/4.0"))
                {
                    if (!_serverBase.ProviderSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = -1;
                        tcpClient.GetStream().WriteTimeout = -1;
                    }
                    client = CreateClientInfo(false, tcpClient, reader);
                    client.ProtocolType = ClientProtocolType.SignalGoOneWay;
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    OneWayServiceProvider.StartToReadingClientData(client, _serverBase);
                }
                else if (firstLineString.Contains("SignalGo/4.0"))
                {
                    client = CreateClientInfo(false, tcpClient, reader);
                    client.ProtocolType = ClientProtocolType.SignalGoDuplex;
                    //"SignalGo/1.0";
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    if (_serverBase.ProviderSetting.ServerServiceSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.ReceiveTimeout = (int)_serverBase.ProviderSetting.ServerServiceSetting.ReceiveDataTimeout.TotalMilliseconds;
                        tcpClient.SendTimeout = (int)_serverBase.ProviderSetting.ServerServiceSetting.SendDataTimeout.TotalMilliseconds;
                    }
                    else
                    {
                        tcpClient.GetStream().ReadTimeout = -1;
                        tcpClient.GetStream().WriteTimeout = -1;
                    }
                    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, _serverBase);
                }
                else if (firstLineString.Contains("HTTP/"))
                {
                    if (_serverBase.ProviderSetting.HttpSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = (int)_serverBase.ProviderSetting.HttpSetting.ReceiveDataTimeout.TotalMilliseconds;
                        tcpClient.GetStream().WriteTimeout = (int)_serverBase.ProviderSetting.HttpSetting.SendDataTimeout.TotalMilliseconds;
                    }
                    await HttpProvider.StartToReadingClientData(tcpClient, _serverBase, reader, new StringBuilder(firstLineString));
                }
                else
                {
                    _serverBase.DisposeClient(client, tcpClient, "AddClient header not support");
                }
            }
            catch (Exception)
            {
                _serverBase.DisposeClient(client, tcpClient, "exception");
            }
            finally
            {
                _WaitingToReadFirstLineCount--;
            }
        }

        public string GetInformation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Clients Connected Count: " + _serverBase.Clients.Count);
            stringBuilder.AppendLine("Http Clients Connected Count: " + _serverBase.Clients.Values.Count(x => x.ProtocolType == ClientProtocolType.Http));
            stringBuilder.AppendLine("SignalGoDuplex Clients Connected Count: " + _serverBase.Clients.Values.Count(x => x.ProtocolType == ClientProtocolType.SignalGoDuplex));
            stringBuilder.AppendLine("SignalGoOneWay Clients Connected Count: " + _serverBase.Clients.Values.Count(x => x.ProtocolType == ClientProtocolType.SignalGoOneWay));
            stringBuilder.AppendLine("SignalGoStream Clients Connected Count: " + _serverBase.Clients.Values.Count(x => x.ProtocolType == ClientProtocolType.SignalGoStream));
            stringBuilder.AppendLine("WebSocket Clients Connected Count: " + _serverBase.Clients.Values.Count(x => x.ProtocolType == ClientProtocolType.WebSocket));
            stringBuilder.AppendLine("None Clients Connected Count: " + _serverBase.Clients.Values.Count(x => x.ProtocolType == ClientProtocolType.None));
#if (!NETSTANDARD1_6)
            stringBuilder.AppendLine("Thread Count: " + System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
#endif
            stringBuilder.AppendLine("ClientServiceCallMethodsResult Count: " + _serverBase.ClientServiceCallMethodsResult.Count);
            stringBuilder.AppendLine("MultipleInstanceServices Count: " + _serverBase.MultipleInstanceServices.Count);
            stringBuilder.AppendLine("SingleInstanceServices Count: " + _serverBase.SingleInstanceServices.Count);
            stringBuilder.AppendLine("TaskOfClientInfoes Count: " + _serverBase.TaskOfClientInfoes.Count);
            stringBuilder.AppendLine("CustomClientSavedSettings Count: " + OperationContextBase.CustomClientSavedSettings.Count);
            stringBuilder.AppendLine("CustomClientSavedSettings Count: " + OperationContextBase.SavedSettings.Count);
            stringBuilder.AppendLine("CachedCustomAttributes Count: " + AttributeHelper.CachedCustomAttributes.Count);
            stringBuilder.AppendLine("CachedTypesOfAttribute Count: " + AttributeHelper.CachedTypesOfAttribute.Count);
            stringBuilder.AppendLine("InheritCachedCustomAttributes Count: " + AttributeHelper.InheritCachedCustomAttributes.Count);
            stringBuilder.AppendLine("CachedMethods Count: " + BaseProvider.CachedMethods.Count);
            stringBuilder.AppendLine("ListOfContextsDataExchangers Count: " + DataExchanger.ListOfContextsDataExchangers.Count);
            stringBuilder.AppendLine("CurrentTaskServerTasks Count: " + OperationContext.CurrentTaskServerTasks.Count);
            stringBuilder.AppendLine("IsWaitForClient: " + IsWaitForClient);
            stringBuilder.AppendLine("IsStarted: " + _serverBase.IsStarted);
            stringBuilder.AppendLine("Connected: " + _server.Server.Connected);
            stringBuilder.AppendLine("Available: " + _server.Server.Available);
            stringBuilder.AppendLine("_ConnectedCount: " + _ConnectedCount);
            stringBuilder.AppendLine("_WaitingToReadFirstLineCount: " + _WaitingToReadFirstLineCount);

            IGrouping<string, ClientInfo> maximumConnectionOfIp = _serverBase.Clients.Values.GroupBy(x => x.IPAddress).OrderByDescending(x => x.Count()).FirstOrDefault();
            if (maximumConnectionOfIp != null)
            {
                stringBuilder.AppendLine("Max ConnectionCount Of Ip: " + maximumConnectionOfIp.Key + " Count:" + maximumConnectionOfIp.Count());

                stringBuilder.AppendLine($"Http {maximumConnectionOfIp.Key} Connected Count: " + maximumConnectionOfIp.Count(x => x.ProtocolType == ClientProtocolType.Http));
                stringBuilder.AppendLine($"SignalGoDuplex {maximumConnectionOfIp.Key} Connected Count: " + maximumConnectionOfIp.Count(x => x.ProtocolType == ClientProtocolType.SignalGoDuplex));
                stringBuilder.AppendLine($"SignalGoOneWay {maximumConnectionOfIp.Key} Connected Count: " + maximumConnectionOfIp.Count(x => x.ProtocolType == ClientProtocolType.SignalGoOneWay));
                stringBuilder.AppendLine($"SignalGoStream {maximumConnectionOfIp.Key} Connected Count: " + maximumConnectionOfIp.Count(x => x.ProtocolType == ClientProtocolType.SignalGoStream));
                stringBuilder.AppendLine($"WebSocket {maximumConnectionOfIp.Key} Connected Count: " + maximumConnectionOfIp.Count(x => x.ProtocolType == ClientProtocolType.WebSocket));
                stringBuilder.AppendLine($"None {maximumConnectionOfIp.Key} Connected Count: " + maximumConnectionOfIp.Count(x => x.ProtocolType == ClientProtocolType.None));


            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// count of connected
        /// </summary>
        /// <returns></returns>
        public int GetConnectedCount()
        {
            return _ConnectedCount;
        }
    }
}
