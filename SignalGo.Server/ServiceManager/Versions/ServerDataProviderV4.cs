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
                if (firstLineString.Contains("SignalGo-Stream/2.0"))
                {
                    //client = CreateClientInfo(false, tcpClient);
                    ////"SignalGo/1.0";
                    ////"SignalGo/1.0";
                    //client.IsWebSocket = false;
                    //var firstByte = GoStreamReader.ReadOneByte(tcpClient.GetStream(), CompressMode.None, 1, false);

                    ////upload from client and download from server
                    //if (firstByte == 0)
                    //{
                    //    DownloadStreamFromClient(tcpClient.GetStream(), client);
                    //}
                    ////download from server and upload from client
                    //else
                    //{
                    //    UploadStreamToClient(tcpClient.GetStream(), client);
                    //}
                    //DisposeClient(client, "AddClient end signalgo stream");
                    //return;
                    throw new NotSupportedException();
                }
                if (firstLineString.Contains("SignalGo-OneWay/2.0"))
                {
                    //client = CreateClientInfo(false, tcpClient);
                    //client.IsWebSocket = false;
                    //OneWayProvider.RunMethod(this, tcpClient.GetStream(), client);
                    //DisposeClient(client, "AddClient end signalgo stream");
                    //return;
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
                    //while (true)
                    //{
                    //    var line = reader.ReadLine();
                    //    firstLineString += line;
                    //    if (line == "\r\n")
                    //        break;
                    //}
                    //if (firstLineString.Contains("Sec-WebSocket-Key"))
                    //{
                    //    client = CreateClientInfo(false, tcpClient);

                    //    client.IsWebSocket = true;
                    //    var key = firstLineString.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
                    //    var acceptKey = AcceptKey(ref key);
                    //    var newLine = "\r\n";

                    //    var response = "HTTP/1.0 101 Switching Protocols" + newLine
                    //     + "Upgrade: websocket" + newLine
                    //     + "Connection: Upgrade" + newLine
                    //     + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;
                    //    var bytes = System.Text.Encoding.UTF8.GetBytes(response);
                    //    tcpClient.GetStream().Write(bytes, 0, bytes.Length);
                    //}
                    //else
                    //{
                    //    client = CreateClientInfo(true, tcpClient);

                    //    string[] lines = null;
                    //    if (firstLineString.Contains("\r\n\r\n"))
                    //        lines = firstLineString.Substring(0, firstLineString.IndexOf("\r\n\r\n")).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    //    else
                    //        lines = firstLineString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    //    var newLine = "\r\n";
                    //    string response = "";
                    //    if (lines.Length > 0)
                    //    {
                    //        var methodName = GetHttpMethodName(lines[0]);
                    //        var address = GetHttpAddress(lines[0]);
                    //        if (methodName.ToLower() == "get" && !string.IsNullOrEmpty(address) && address != "/")
                    //        {
                    //            var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                    //            if (headers["content-type"] != null && headers["content-type"] == "SignalGo Service Reference")
                    //            {
                    //                var doClient = (HttpClientInfo)client;
                    //                doClient.RequestHeaders = headers;
                    //                SendSignalGoServiceReference(doClient);
                    //            }
                    //            else
                    //                RunHttpRequest(address, "GET", "", headers, (HttpClientInfo)client);
                    //            DisposeClient(client, "AddClient finish get call");
                    //            return;
                    //        }
                    //        else if (methodName.ToLower() == "post" && !string.IsNullOrEmpty(address) && address != "/")
                    //        {
                    //            var indexOfStartedContent = firstLineString.IndexOf("\r\n\r\n");
                    //            string content = "";
                    //            if (indexOfStartedContent > 0)
                    //            {
                    //                indexOfStartedContent += 4;
                    //                content = firstLineString.Substring(indexOfStartedContent, firstLineString.Length - indexOfStartedContent);
                    //            }
                    //            var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                    //            if (headers["content-type"] != null && headers["content-type"].ToLower().Contains("multipart/form-data"))
                    //            {
                    //                RunPostHttpRequestFile(address, "POST", content, headers, (HttpClientInfo)client);
                    //            }
                    //            else if (headers["content-type"] != null && headers["content-type"] == "SignalGo Service Reference")
                    //            {
                    //                SendSignalGoServiceReference((HttpClientInfo)client);
                    //                return;
                    //            }
                    //            else
                    //            {
                    //                RunHttpRequest(address, "POST", content, headers, (HttpClientInfo)client);
                    //            }
                    //            DisposeClient(client, "AddClient finish post call");
                    //            return;
                    //        }
                    //        else if (methodName.ToLower() == "options" && !string.IsNullOrEmpty(address) && address != "/")
                    //        {
                    //            string settingHeaders = "";
                    //            var headers = GetHttpHeaders(lines.Skip(1).ToArray());

                    //            if (HttpProtocolSetting != null)
                    //            {
                    //                if (HttpProtocolSetting.HandleCrossOriginAccess)
                    //                {
                    //                    settingHeaders = "Access-Control-Allow-Origin: " + headers["origin"] + newLine +
                    //                    "Access-Control-Allow-Credentials: true" + newLine
                    //                    //"Access-Control-Allow-Methods: " + "POST,GET,OPTIONS" + newLine
                    //                    ;

                    //                    if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                    //                    {
                    //                        settingHeaders += "Access-Control-Allow-Headers: " + headers["Access-Control-Request-Headers"] + newLine;
                    //                    }
                    //                }
                    //            }
                    //            string message = newLine + $"Success" + newLine;
                    //            response = $"HTTP/1.1 {(int)HttpStatusCode.OK} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.OK)}" + newLine
                    //                + "Content-Type: text/html; charset=utf-8" + newLine
                    //                + settingHeaders
                    //                + "Connection: Close" + newLine;
                    //            client.TcpClient.Client.Send(System.Text.Encoding.UTF8.GetBytes(response + message));
                    //            DisposeClient(client, "AddClient finish post call");
                    //            return;
                    //        }
                    //        else if (RegisteredHttpServiceTypes.ContainsKey("") && (string.IsNullOrEmpty(address) || address == "/"))
                    //        {
                    //            var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                    //            RunIndexHttpRequest(headers, (HttpClientInfo)client);
                    //            DisposeClient(client, "Index Page call");
                    //            return;
                    //        }
                    //    }

                    //    response = "HTTP/1.1 200 OK" + newLine
                    //         + "Content-Type: text/html" + newLine
                    //         + "Connection: Close" + newLine;
                    //    tcpClient.Client.Send(System.Text.Encoding.ASCII.GetBytes(response + newLine + "SignalGo Server OK" + newLine));
                    //    DisposeClient(client, "AddClient http ok signalGo");
                    //    return;
                    //}

                }
                else
                {
                    //if (firstLineString == null)
                    //    firstLineString = "";
                    //if (reader.LastByteRead >= 0)
                    //    _serverBase.AutoLogger.LogText($"Header not suport msg: {firstLineString} {(client == null ? "null" : client.IPAddress)} IsConnected:{(client == null ? "null" : client.TcpClient.Connected.ToString())} LastByte:{reader.LastByteRead}");

                    //DisposeClient(client, "AddClient header not support");
                    //return;
                }

                //StartToReadingClientData(client);
                //OnConnectedClientAction?.Invoke(client);
            }
            catch (Exception)
            {
                _serverBase.DisposeClient(client, "exception");
            }
        }
    }
}
