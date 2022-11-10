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

        private volatile int _ConnectedCount;
        private volatile int _WaitingToReadFirstLineCount;
        /// <summary>
        /// if you set this to false server will reject and discounnect the new client until to set it true
        /// </summary>
        public bool CanAcceptClient { get; set; } = true;
        public void Start(ServerBase serverBase, int port)
        {
            Thread thread = new Thread(async () =>
            {
                try
                {
                    await StartInternal(serverBase, port);
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
                await AcceptServerClients(serverBase, port);
            })
            {
                IsBackground = false
            };
            thread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task StartAsync(ServerBase serverBase, int port)
        {
            await StartInternal(serverBase, port);
            _ = Task.Factory.StartNew(async () =>
            {
                await AcceptServerClients(serverBase, port);
            });
        }

        private async Task StartInternal(ServerBase serverBase, int port)
        {
            _serverBase = serverBase;
            _server = new TcpListener(IPAddress.IPv6Any, port);
#if (NET35)
#else
            _server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
#endif
            _server.Server.NoDelay = true;

            _server.Start();
        }

        private async Task AcceptServerClients(ServerBase serverBase, int port)
        {
            try
            {
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
                        Debug.WriteLine("DeadLock Warning Start Server!");
                        TcpClient client = _server.AcceptTcpClientAsync().ConfigureAwait(false).GetAwaiter().GetResult();
#elif (NETSTANDARD2_0 || NET6_0)
                        TcpClient client = await _server.AcceptTcpClientAsync().ConfigureAwait(false);
#else

                        TcpClient client = _server.AcceptTcpClient();

#endif

                        if (!CanAcceptClient)
                        {
#if (NETSTANDARD1_6)
                            client.Dispose();
#elif (NETSTANDARD2_0 || NET6_0)
                            client.Close();
                            client.Dispose();
#else
                            client.Close();
#endif
                            continue;
                        }
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
                    if (await _serverBase.Firewall.OnTcpClientConnected(tcpClient).ConfigureAwait(false))
                    {
                        tcpClient.GetStream().ReadTimeout = 5000;
                        tcpClient.GetStream().WriteTimeout = 5000;
                        PipeNetworkStream stream = new PipeNetworkStream(new NormalStream(await tcpClient.GetTcpStream(_serverBase).ConfigureAwait(false)), (int)_serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds);
                        ExchangeClient(stream, tcpClient);
                    }
                    else
                    {
#if (NETSTANDARD)
                        tcpClient.Dispose();
#else
                        tcpClient.Close();
#endif
                    }
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
            bool isReadFirstLine = false;
            try
            {
                _WaitingToReadFirstLineCount++;
                if (_serverBase.ProviderSetting.IsEnabledToUseTimeout)
                {
                    tcpClient.GetStream().ReadTimeout = (int)_serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                    tcpClient.GetStream().WriteTimeout = (int)_serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
                }

                reader.MaximumLineSize = _serverBase.ProviderSetting.MaximumLineSize;
                if (reader.MaximumLineSize > 0)
                {
                    reader.MaximumLineSizeReadedFunction = () =>
                    {
                        return _serverBase.Firewall.OnDangerDataReceived(tcpClient, client, Firewall.DangerDataType.FirstLineSize);
                    };
                }

                string firstLineString = await reader.ReadLineAsync().ConfigureAwait(false);
                isReadFirstLine = true;
                _WaitingToReadFirstLineCount--;

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
                    if (await _serverBase.Firewall.OnClientInitialized(client).ConfigureAwait(false))
                        SignalGoStreamProvider.StartToReadingClientData(client, _serverBase);
                    else
                        _serverBase.DisposeClient(client, tcpClient, "firewall dropped!");

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
                    if (await _serverBase.Firewall.OnClientInitialized(client).ConfigureAwait(false))
                        OneWayServiceProvider.StartToReadingClientData(client, _serverBase);
                    else
                        _serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
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
                    if (await _serverBase.Firewall.OnClientInitialized(client).ConfigureAwait(false))
                        await SignalGoDuplexServiceProvider.StartToReadingClientData(client, _serverBase).ConfigureAwait(false);
                    else
                        _serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                }
                else if (firstLineString.Contains("HTTP/"))
                {
                    if (_serverBase.ProviderSetting.HttpSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = (int)_serverBase.ProviderSetting.HttpSetting.ReceiveDataTimeout.TotalMilliseconds;
                        tcpClient.GetStream().WriteTimeout = (int)_serverBase.ProviderSetting.HttpSetting.SendDataTimeout.TotalMilliseconds;
                    }
                    await HttpProvider.StartToReadingClientData(tcpClient, _serverBase, reader, new StringBuilder(firstLineString)).ConfigureAwait(false);
                }
                else
                {
                    _serverBase.DisposeClient(client, tcpClient, "AddClient header not support");
                }
            }
            catch (Exception ex)
            {
                _serverBase.DisposeClient(client, tcpClient, "exception");
            }
            finally
            {
                if (!isReadFirstLine)
                    _WaitingToReadFirstLineCount--;
                _ConnectedCount--;
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
            stringBuilder.AppendLine("ClientServiceCallMethods Count: " + _serverBase.ClientServiceCallMethods.Count);

            stringBuilder.AppendLine("MultipleInstanceServices Count: " + _serverBase.MultipleInstanceServices.Count);
            stringBuilder.AppendLine("SingleInstanceServices Count: " + _serverBase.SingleInstanceServices.Count);
            stringBuilder.AppendLine("TaskOfClientInfoes Count: " + _serverBase.TaskOfClientInfoes.Count);
            stringBuilder.AppendLine("CustomClientSavedSettings Count: " + OperationContextBase.CustomClientSavedSettings.Count);
            stringBuilder.AppendLine("CustomClientSavedSettings Count: " + OperationContextBase.SavedSettings.Count);
            //stringBuilder.AppendLine("CachedCustomAttributes Count: " + AttributeHelper.CachedCustomAttributes.Count);
            stringBuilder.AppendLine("CachedTypesOfAttribute Count: " + AttributeHelper.CachedTypesOfAttribute.Count);
            //stringBuilder.AppendLine("InheritCachedCustomAttributes Count: " + AttributeHelper.InheritCachedCustomAttributes.Count);
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

        string GetClientKey(ClientInfo clientInfo)
        {
            return $"{clientInfo.ProtocolType}_{clientInfo.LevelFlag}_{clientInfo.IPAddress}_{clientInfo.ForwardIPAddress}";
        }

        public string GetClientInformation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in _serverBase.Clients.Values.OrderBy(x => GetClientKey(x)).ToList())
            {
                stringBuilder.AppendLine("Client");
                stringBuilder.AppendLine(GetClientKey(item));
                stringBuilder.AppendLine(item.LevelFlag);
                stringBuilder.AppendLine(item.ProtocolType.ToString());
                stringBuilder.AppendLine(item.IPAddress);
                stringBuilder.AppendLine(item.ConnectedDateTime.ToString());
                stringBuilder.AppendLine(item.ForwardIPAddress);
                stringBuilder.AppendLine("End");
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
