using SignalGo.Server.IO;
using SignalGo.Server.Models;
using SignalGo.Shared.Log;
using SignalGo.Shared.Managers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SignalGo.Server.ServiceManager
{
    /// <summary>
    /// status of server
    /// </summary>
    public enum ServerStatus : byte
    {
        /// <summary>
        /// nothing
        /// </summary>
        None = 0,
        /// <summary>
        /// started
        /// </summary>
        Started = 1,
        /// <summary>
        /// stopped
        /// </summary>
        Stopped = 2
    }
    /// <summary>
    /// base of server side system
    /// </summary>
    public class ServerBaseNew
    {

        #region Actions
        /// <summary>
        /// when server disconnect
        /// </summary>
        public Action<ServerStatus> OnServerStatusChangedAction { get; set; }
        /// <summary>
        /// after client connected
        /// </summary>
        public Action<ClientInfo> OnConnectedClientAction { get; set; }
        /// <summary>
        /// after client disconnected
        /// </summary>
        public Action<ClientInfo> OnDisconnectedClientAction { get; set; }

        #endregion

        /// <summary>
        /// settings of server
        /// </summary>
        public ProviderSetting ProviderSetting { get; set; } = new ProviderSetting();
        /// <summary>
        /// default log system of server
        /// </summary>
        internal AutoLogger AutoLogger { get; private set; } = new AutoLogger() { FileName = "ServerBase Logs.log" };


        /// <summary>
        /// all items in server like clients,ids,dispatchers etc
        /// </summary>
        internal UltraMapDictionary AllItems { get; set; } = new UltraMapDictionary();

        /// <summary>
        /// list of registred types of service like http services,server services and client services
        /// httpService naming: HTTP + service name
        /// serverService naming SERVER + service name 
        /// clientService naming CLIENT + service name
        /// streamService naming STREAM + service name
        /// dic key is service name second key is type of service and value is instance of service
        /// </summary>
        internal ConcurrentDictionary<string, KeyValue<Type, object>> RegisteredServiceTypes { get; set; } = new ConcurrentDictionary<string, KeyValue<Type, object>>();
        /// <summary>
        /// virtual urls that your server binded
        /// </summary>
        internal List<string> VirtualDirectories { get; set; } = new List<string>();
        /// <summary>
        /// segment manager t manage partion of method calls
        /// </summary>
        internal SegmentManager CurrentSegmentManager = new SegmentManager();
        /// <summary>
        /// status of server
        /// </summary>
        public ServerStatus Status { get; private set; }
        private volatile bool _IsFinishingServer = false;
        /// <summary>
        /// is server going to finish
        /// </summary>
        public bool IsFinishingServer
        {
            get
            {
                return _IsFinishingServer;
            }
            set
            {
                _IsFinishingServer = value;
            }
        }

        TcpListener server = null;
        Thread mainThread = null;

        /// <summary>
        /// start the server
        /// </summary>
        /// <param name="port"></param>
        /// <param name="virtualUrl"></param>
        internal void Connect(int port, string[] virtualUrl)
        {
            Exception exception = null;
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            mainThread = new Thread(() =>
            {
                try
                {
                    server = new TcpListener(IPAddress.IPv6Any, port);
#if (NET35)
                    //server.Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IP, false);
#else
                    server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
#endif
                    server.Server.NoDelay = true;

                    foreach (var item in virtualUrl)
                    {
                        if (!VirtualDirectories.Contains(item))
                            VirtualDirectories.Add(item);
                    }

                    server.Start();
                    if (ProviderSetting.IsEnabledToUseTimeout)
                    {
                        server.Server.SendTimeout = (int)ProviderSetting.SendDataTimeout.TotalMilliseconds;
                        server.Server.ReceiveTimeout = (int)ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                    }

                    Status = ServerStatus.Started;
                    OnServerStatusChangedAction?.Invoke(Status);
                    resetEvent.Set();
                    while (true)
                    {
                        AcceptTcpClient();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server Disposed! : " + ex);
                    AutoLogger.LogError(ex, "Connect Server");
                    exception = ex;
                    resetEvent.Set();
                    Stop();
                    Status = ServerStatus.Stopped;
                    OnServerStatusChangedAction?.Invoke(Status);
                }
            })
            {
                IsBackground = false
            };
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
#else
            mainThread.SetApartmentState(ApartmentState.STA);
#endif
            mainThread.Start();
            resetEvent.WaitOne();
            if (exception != null)
                throw exception;
        }

        
        internal async void AcceptTcpClient()
        { 
            AddClient(await server.AcceptSocketAsync());
        }


        /// <summary>
        /// when client connected to server
        /// </summary>
        /// <param name="socket">client</param>
        private void AddClient(Socket socket)
        {
            LogClients();
            if (IsFinishingServer)
            {
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                socket.Dispose();
#else
                socket.Close();
#endif
                return;
            }
            SocketDataProvider socketDataProvider = new SocketDataProvider(socket);
            AsyncActions.Run(() =>
            {
                string headerResponse = "";
                ClientInfo client = null;
                try
                {
                    using (var reader = new CustomStreamReader(socket.GetStream()))
                    {
                        headerResponse = reader.ReadLine();

                        if (headerResponse.Contains("SignalGo-Stream/2.0"))
                        {
                            client = CreateClientInfo(false, socket);
                            //"SignalGo/1.0";
                            //"SignalGo/1.0";
                            client.IsWebSocket = false;
                            var b = GoStreamReader.ReadOneByte(socket.GetStream(), CompressMode.None, 1, false);
                            if (SynchronizationContext.Current == null)
                                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

                            AllDispatchers.Add(SynchronizationContext.Current, client);
                            client.MainContext = SynchronizationContext.Current;
                            client.MainThread = System.Threading.Thread.CurrentThread;

                            //upload from client and download from server
                            if (b == 0)
                            {
                                DownloadStreamFromClient(socket.GetStream(), client);
                            }
                            //download from server and upload from client
                            else
                            {
                                UploadStreamToClient(socket.GetStream(), client);
                            }
                            DisposeClient(client, "AddClient end signalgo stream");
                            return;
                        }
                        else if (headerResponse.Contains("SignalGo/1.0"))
                        {
                            client = CreateClientInfo(false, socket);
                            //"SignalGo/1.0";
                            //"SignalGo/1.0";
                            client.IsWebSocket = false;
                            var bytes = System.Text.Encoding.UTF8.GetBytes("OK");
                            socket.GetStream().Write(bytes, 0, bytes.Length);
                        }
                        else if (headerResponse.Contains("HTTP/1.1") || headerResponse.Contains("HTTP/1.0"))
                        {
                            while (true)
                            {
                                var line = reader.ReadLine();
                                headerResponse += line;
                                if (line == "\r\n")
                                    break;
                            }
                            if (headerResponse.Contains("Sec-WebSocket-Key"))
                            {
                                client = CreateClientInfo(false, socket);
                                //Console.WriteLine($"WebSocket client detected : {client.IPAddress} {client.ClientId} {DateTime.Now.ToString()} {ClientConnectedCallingCount}");

                                client.IsWebSocket = true;
                                var key = headerResponse.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
                                var acceptKey = AcceptKey(ref key);
                                var newLine = "\r\n";

                                //var response = "HTTP/1.1 101 Switching Protocols" + newLine
                                var response = "HTTP/1.0 101 Switching Protocols" + newLine
                                 + "Upgrade: websocket" + newLine
                                 + "Connection: Upgrade" + newLine
                                 + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;
                                var bytes = System.Text.Encoding.UTF8.GetBytes(response);
                                socket.GetStream().Write(bytes, 0, bytes.Length);
                                //Console.WriteLine($"WebSocket client send reponse success size:{bytes.Length} sended{count}");
                            }
                            else
                            {
                                client = CreateClientInfo(true, socket);

                                if (SynchronizationContext.Current == null)
                                    SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                                AllDispatchers.Add(SynchronizationContext.Current, client);

                                string[] lines = null;
                                if (headerResponse.Contains("\r\n\r\n"))
                                    lines = headerResponse.Substring(0, headerResponse.IndexOf("\r\n\r\n")).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                else
                                    lines = headerResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                var newLine = "\r\n";
                                string response = "";
                                if (lines.Length > 0)
                                {
                                    var methodName = GetHttpMethodName(lines[0]);
                                    var address = GetHttpAddress(lines[0]);
                                    if (methodName.ToLower() == "get" && !string.IsNullOrEmpty(address) && address != "/")
                                    {
                                        var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                                        if (headers["content-type"] != null && headers["content-type"] == "SignalGo Service Reference")
                                        {
                                            var doClient = (HttpClientInfo)client;
                                            doClient.RequestHeaders = headers;
                                            SendSignalGoServiceReference(doClient);
                                        }
                                        else
                                            RunHttpRequest(address, "GET", "", headers, (HttpClientInfo)client);
                                        DisposeClient(client, "AddClient finish get call");
                                        return;
                                    }
                                    else if (methodName.ToLower() == "post" && !string.IsNullOrEmpty(address) && address != "/")
                                    {
                                        var indexOfStartedContent = headerResponse.IndexOf("\r\n\r\n");
                                        string content = "";
                                        if (indexOfStartedContent > 0)
                                        {
                                            indexOfStartedContent += 4;
                                            content = headerResponse.Substring(indexOfStartedContent, headerResponse.Length - indexOfStartedContent);
                                        }
                                        var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                                        if (headers["content-type"] != null && headers["content-type"].ToLower().Contains("multipart/form-data"))
                                        {
                                            RunPostHttpRequestFile(address, "POST", content, headers, (HttpClientInfo)client);
                                        }
                                        else if (headers["content-type"] != null && headers["content-type"] == "SignalGo Service Reference")
                                        {
                                            SendSignalGoServiceReference((HttpClientInfo)client);
                                            return;
                                        }
                                        else
                                        {
                                            RunHttpRequest(address, "POST", content, headers, (HttpClientInfo)client);
                                        }
                                        DisposeClient(client, "AddClient finish post call");
                                        return;
                                    }
                                    else if (methodName.ToLower() == "options" && !string.IsNullOrEmpty(address) && address != "/")
                                    {
                                        string settingHeaders = "";
                                        var headers = GetHttpHeaders(lines.Skip(1).ToArray());

                                        if (HttpProtocolSetting != null)
                                        {
                                            if (HttpProtocolSetting.HandleCrossOriginAccess)
                                            {
                                                settingHeaders = "Access-Control-Allow-Origin: " + headers["origin"] + newLine +
                                                "Access-Control-Allow-Credentials: true" + newLine
                                                //"Access-Control-Allow-Methods: " + "POST,GET,OPTIONS" + newLine
                                                ;

                                                if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                                                {
                                                    settingHeaders += "Access-Control-Allow-Headers: " + headers["Access-Control-Request-Headers"] + newLine;
                                                }
                                            }
                                        }
                                        string message = newLine + $"Success" + newLine;
                                        response = $"HTTP/1.1 {(int)HttpStatusCode.OK} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.OK)}" + newLine
                                            + "Content-Type: text/html; charset=utf-8" + newLine
                                            + settingHeaders
                                            + "Connection: Close" + newLine;
                                        client.TcpClient.Client.Send(System.Text.Encoding.UTF8.GetBytes(response + message));
                                        DisposeClient(client, "AddClient finish post call");
                                        return;
                                    }
                                    else if (RegisteredHttpServiceTypes.ContainsKey("") && (string.IsNullOrEmpty(address) || address == "/"))
                                    {
                                        var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                                        RunIndexHttpRequest(headers, (HttpClientInfo)client);
                                        DisposeClient(client, "Index Page call");
                                        return;
                                    }
                                }

                                response = "HTTP/1.1 200 OK" + newLine
                                     + "Content-Type: text/html" + newLine
                                     + "Connection: Close" + newLine;
                                socket.Client.Send(System.Text.Encoding.ASCII.GetBytes(response + newLine + "SignalGo Server OK" + newLine));
                                DisposeClient(client, "AddClient http ok signalGo");
                                return;
                            }

                        }
                        else
                        {
                            if (headerResponse == null)
                                headerResponse = "";
                            if (reader.LastByteRead >= 0)
                                AutoLogger.LogText($"Header not suport msg: {headerResponse} {(client == null ? "null" : client.IPAddress)} IsConnected:{(client == null ? "null" : client.TcpClient.Connected.ToString())} LastByte:{reader.LastByteRead}");

                            DisposeClient(client, "AddClient header not support");
                            return;
                        }

                        StartToReadingClientData(client);
                        OnConnectedClientAction?.Invoke(client);
                    }
                }
                catch (Exception ex)
                {
                    if (headerResponse == null)
                        headerResponse = "";
                    if (!(ex is System.Net.Sockets.SocketException) && !(ex is System.IO.IOException) && client != null)
                    {
                        AutoLogger.LogText($"AddClient Error msg : {headerResponse} {(client == null ? "null client" : client.IPAddress)}");
                        AutoLogger.LogError(ex, "AddClient");
                        //Console.WriteLine(ex);
                    }
                    DisposeClient(client, "AddClient exception");
                }
            });
        }
    }
}
