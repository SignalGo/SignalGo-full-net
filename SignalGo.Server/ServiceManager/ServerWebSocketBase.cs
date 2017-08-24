using Newtonsoft.Json;
using SignalGo.Server.Models;
using SignalGo.Shared;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SignalGo.Server.ServiceManager
{
    //public abstract class ServerWebSocketBase : ServerBase
    //{
    //    TcpListener server = null;
    //    private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    //    /// <summary>
    //    /// start with web socket protocol
    //    /// </summary>
    //    internal void ConnectWebSocket(int port, string[] virtualUrl)
    //    {
    //        Exception exception = null;
    //        AutoResetEvent resetEvent = new AutoResetEvent(false);
    //        AsyncActions.Run(() =>
    //        {
    //            try
    //            {
    //                server = new TcpListener(IPAddress.IPv6Any, port);
    //                server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
    //                foreach (var item in virtualUrl)
    //                {
    //                    if (!VirtualDirectories.Contains(item))
    //                        VirtualDirectories.Add(item);
    //                }
    //                server.Start();
    //                resetEvent.Set();
    //                while (true)
    //                {
    //                    AddWebSocketClient(server.AcceptTcpClient());
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                SignalGo.Shared.Log.AutoLogger.LogError(ex, "Connect");
    //                exception = ex;
    //                resetEvent.Set();
    //            }
    //        });
    //        resetEvent.WaitOne();
    //        if (exception != null)
    //            throw exception;
    //    }

    //    private void AddWebSocketClient(TcpClient tcpClient)
    //    {
    //        ClientInfo client = new ClientInfo() { IsWebSocket = true };
    //        client.ServerBase = this;
    //        client.TcpClient = tcpClient;
    //        client.IPAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Replace("::ffff:", "");

    //        client.SessionId = Guid.NewGuid().ToString();
    //        Clients.Add(client);
    //        if (ClientsByIp.ContainsKey(client.IPAddress))
    //            ClientsByIp[client.IPAddress].Add(client);
    //        else
    //            ClientsByIp.TryAdd(client.IPAddress, new List<Models.ClientInfo>() { client });
    //        Services.TryAdd(client, new ConcurrentList<object>());
    //        WaitedMethodsForResponse.TryAdd(client, new ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>>());
    //        ClientRegistredMethods.TryAdd(client, new ConcurrentDictionary<string, ConcurrentList<string>>());


    //        byte[] buffer = new byte[1024];
    //        var readCount = tcpClient.Client.Receive(buffer);
    //        var headerResponse = System.Text.Encoding.UTF8.GetString(buffer.ToList().GetRange(0, readCount).ToArray());
    //        var key = headerResponse.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
    //        var acceptKey = AcceptKey(ref key);
    //        var newLine = "\r\n";

    //        var response = "HTTP/1.1 101 Switching Protocols" + newLine
    //             + "Upgrade: websocket" + newLine
    //             + "Connection: Upgrade" + newLine
    //             + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;

    //        tcpClient.Client.Send(System.Text.Encoding.UTF8.GetBytes(response));

    //        StartToReadingWebSocketClientData(client);
    //        AddedClient?.Invoke(client);

    //        //byte[] bytes = new byte[4 + 6];
    //        //var dataCount = client.Client.Receive(bytes);
    //        //var decode = DecodeMessage(bytes);
    //        //Console.ReadLine();
    //    }

    //    private void StartToReadingWebSocketClientData(ClientInfo client)
    //    {
    //        Thread thread = new Thread(() =>
    //        {
    //            if (SynchronizationContext.Current == null)
    //                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    //            //ClientDispatchers.TryAdd(client, SynchronizationContext.Current);
    //            AllDispatchers.TryAdd(SynchronizationContext.Current, client);
    //            client.MainThread = SynchronizationContext.Current;
    //            try
    //            {
    //                RegisterCallbacksForClient(client);
    //                var stream = client.TcpClient.GetStream();
    //                bool isVerify = false;
    //                if (!client.IsVerification)
    //                {
    //                    while (client.TcpClient.Connected)
    //                    {
    //                        var bytes = GoStreamReader.ReadBlockToEnd(stream, CompressMode.None, 2048, true);
    //                        var json = Encoding.UTF8.GetString(bytes);
    //                        List<string> registers = JsonConvert.DeserializeObject<List<string>>(json);
    //                        foreach (var item in registers)
    //                        {
    //                            if (VirtualDirectories.Contains(item) || item == "/DownloadFile" || item == "/UploadFile")
    //                            {
    //                                isVerify = true;
    //                                break;
    //                            }
    //                        }
    //                        break;
    //                    }
    //                }
    //                if (!isVerify)
    //                {
    //                    DisposeClient(client);
    //                    return;
    //                }
    //                client.IsVerification = true;
    //                while (client.TcpClient.Connected)
    //                {
    //                    //بایت اول نوع دیتا
    //                    var dataType = (DataType)GoStreamReader.ReadBlockToEnd(stream, CompressMode.None, ProviderSetting.MaximumReceiveDataBlock, true).FirstOrDefault();
    //                    //بایت دوم نوع فشرده سازی
    //                    var compressMode = (CompressMode)GoStreamReader.ReadBlockToEnd(stream, CompressMode.None, ProviderSetting.MaximumReceiveDataBlock, true).FirstOrDefault();
    //                    //یکی از متد های سرور توسط این کلاینت صدا زده شده
    //                    if (dataType == DataType.CallMethod)
    //                    {
    //                        var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, true);
    //                        var json = Encoding.UTF8.GetString(bytes);

    //                        MethodCallInfo callInfo = JsonConvert.DeserializeObject<MethodCallInfo>(json);
    //                        //بررسی آدرس اتصال
    //                        if (callInfo.ServiceName == "/CheckConnection")
    //                            SendCallbackData(new MethodCallbackInfo() { Guid = callInfo.Guid, Data = JsonConvert.SerializeObject(true) }, client);
    //                        //کلاسی کالبکی که سمت سرور جدید میشه
    //                        else if (callInfo.MethodName == "/RegisterService")
    //                            RegisterClassForClient(callInfo, client);
    //                        //متد هایی که لازمه برای کلاینت کال بشه
    //                        else if (callInfo.MethodName == "/RegisterClientMethods")
    //                        {
    //                            RegisterMethodsForClient(callInfo, client);
    //                        }
    //                        //حذف متد هایی که قبلا رجیستر شده بود
    //                        else if (callInfo.MethodName == "/UnRegisterClientMethods")
    //                        {
    //                            UnRegisterMethodsForClient(callInfo, client);
    //                        }
    //                        else
    //                            CallMethod(callInfo, client);
    //                    }
    //                    //پاسخ دریافت شده از صدا زدن یک متد از کلاینت توسط سرور
    //                    else if (dataType == DataType.ResponseCallMethod)
    //                    {
    //                        var bytes = GoStreamReader.ReadBlockToEnd(stream, compressMode, ProviderSetting.MaximumReceiveDataBlock, true);
    //                        var json = Encoding.UTF8.GetString(bytes);
    //                        MethodCallbackInfo callback = JsonConvert.DeserializeObject<MethodCallbackInfo>(json);

    //                        ConcurrentDictionary<string, KeyValue<AutoResetEvent, MethodCallbackInfo>> keyValue = null;
    //                        var geted = WaitedMethodsForResponse.TryGetValue(client, out keyValue);
    //                        if (geted)
    //                        {
    //                            keyValue[callback.Guid].Value = callback;
    //                            keyValue[callback.Guid].Key.Set();
    //                        }
    //                    }
    //                    else if (dataType == DataType.RegisterFileDownload)
    //                    {
    //                        using (var writeToClientStrem = RegisterFileToDownload(stream, compressMode, client, true))
    //                        {
    //                            WriteStreamToClient(writeToClientStrem, stream, client.IsWebSocket);
    //                        }
    //                        DisposeClient(client);
    //                    }
    //                    else if (dataType == DataType.RegisterFileUpload)
    //                    {
    //                        RegisterFileToUpload(stream, compressMode, client, true);
    //                        DisposeClient(client);
    //                    }
    //                    else
    //                    {
    //                        DisposeClient(client);
    //                    }
    //                }
    //                DisposeClient(client);
    //            }
    //            catch (Exception ex)
    //            {
    //                SignalGo.Shared.Log.AutoLogger.LogError(ex, "ServerBase StartToReadingClientData");
    //                DisposeClient(client);
    //            }
    //        });
    //        thread.IsBackground = false;
    //        thread.Start();
    //    }

    //    private string AcceptKey(ref string key)
    //    {
    //        string longKey = key + guid;
    //        byte[] hashBytes = ComputeHash(longKey);
    //        return Convert.ToBase64String(hashBytes);
    //    }

    //    static SHA1 sha1 = SHA1.Create();
    //    private static byte[] ComputeHash(string str)
    //    {
    //        return sha1.ComputeHash(Encoding.ASCII.GetBytes(str));
    //    }
    //}
}
