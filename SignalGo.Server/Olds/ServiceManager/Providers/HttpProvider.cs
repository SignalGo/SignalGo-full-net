using SignalGo.Server.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    /// <summary>
    /// manage data providing of http and https services
    /// </summary>
    public class HttpProvider : BaseHttpProvider
    {
#if (NET35 || NET40)
        public static Task AddHttpClient(HttpClientInfo client, ServerBase serverBase, string address, string methodName, IDictionary<string, string[]> requestHeaders, IDictionary<string, string[]> responseHeaders)
#else
        public static async Task AddHttpClient(HttpClientInfo client, ServerBase serverBase, string address, string methodName, IDictionary<string, string[]> requestHeaders, IDictionary<string, string[]> responseHeaders)
#endif
        {
#if (NET35 || NET40)
            return Task.Factory.StartNew(() =>
#else
            await Task.Run(async () =>
#endif
            {
                try
                {
                    if (requestHeaders != null)
                        client.RequestHeaders = requestHeaders;
                    if (responseHeaders != null)
                        client.ResponseHeaders = responseHeaders;
                    await HandleHttpRequest(methodName, address, serverBase, client);
                }
                catch (Exception ex)
                {
                    if (client.IsOwinClient)
                        throw;
                    serverBase.DisposeClient(client, null, "HttpProvider AddHttpClient exception");
                }
            });
        }

#if (NET35 || NET40)
        public static Task AddWebSocketHttpClient(ClientInfo client, ServerBase serverBase)
#else
        public static Task AddWebSocketHttpClient(ClientInfo client, ServerBase serverBase)
#endif
        {
#if (NET35 || NET40)
            return Task.Factory.StartNew(() =>
#else
            return Task.Run(async () =>
#endif
            {
                try
                {
                    client.IsWebSocket = true;
                    await WebSocketProvider.StartToReadingClientData(client, serverBase);
                }
                catch (Exception ex)
                {
                    serverBase.DisposeClient(client, null, "HttpProvider AddWebSocketHttpClient exception");
                }
            });
        }

#if (NET35 || NET40)
        public static Task AddSignalGoWebSocketHttpClient(ClientInfo client, ServerBase serverBase)
#else
        public static Task AddSignalGoWebSocketHttpClient(ClientInfo client, ServerBase serverBase)
#endif
        {
#if (NET35 || NET40)
            return Task.Factory.StartNew(() =>
#else
            return Task.Run(async () =>
#endif
            {
                try
                {
                    client.IsWebSocket = true;
                    await SignalgoWebSocketProvider.StartToReadingClientData(client, serverBase);
                }
                catch (Exception ex)
                {
                    serverBase.DisposeClient(client, null, "HttpProvider AddWebSocketHttpClient exception");
                }
            });
        }

        public static async Task StartToReadingClientData(TcpClient tcpClient, ServerBase serverBase, PipeLineStream reader, ClientInfo client)
        {
            //Console.WriteLine($"Http Client Connected: {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Replace("::ffff:", "")}");
            try
            {
                //if (reader.Contains("Sec-WebSocket-Key"))
                //{
                //    ////tcpClient.ReceiveTimeout = -1;
                //    ////tcpClient.SendTimeout = -1;
                //    //client = serverBase.ServerDataProvider.c(serverBase, tcpClient, reader);
                //    //client.ProtocolType = ClientProtocolType.WebSocket;
                //    //client.IsWebSocket = true;
                //    //string key = requestHeaders.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
                //    //string acceptKey = AcceptKey(ref key);
                //    //string newLine = TextHelper.NewLine;

                //    ////var response = "HTTP/1.1 101 Switching Protocols" + newLine
                //    //string response = "HTTP/1.0 101 Switching Protocols" + newLine
                //    // + "Upgrade: websocket" + newLine
                //    // + "Connection: Upgrade" + newLine
                //    // + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;
                //    //byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
                //    //await client.ClientStream.WriteAsync(bytes, 0, bytes.Length);
                //    //client.StreamHelper = SignalGoStreamBase.CurrentBase;
                //    //client.ClientStream = new PipeNetworkStream(new WebSocketStream(client.TcpClient.GetStream()));
                //    //if (requestHeaders.Contains("SignalgoDuplexWebSocket"))
                //    //{

                //    //    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase);
                //    //}
                //    //else
                //    //{
                //    //    //client.StreamHelper = SignalGoStreamWebSocketLlight.CurrentWebSocket;
                //    //    //client.ClientStream = new PipeNetworkStream(new WebSocketStream(client.TcpClient.GetStream()));
                //    //    //await WebSocketProvider.StartToReadingClientData(client, serverBase);

                //    //    await HttpProvider.AddWebSocketHttpClient(client, serverBase);
                //    //}
                //}
                //else if (requestHeaders.Contains("SignalGoHttpDuplex"))
                //{
                //    client = serverBase.ServerDataProvider.CreateClientInfo(false, tcpClient, reader);
                //    client.ProtocolType = ClientProtocolType.HttpDuplex;
                //    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                //    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase);
                //}
                //else
                //{
                try
                {
                    client.ClientStream = reader;
                    //serverBase.TaskOfClientInfoes
                    //client = (HttpClientInfo)serverBase.ServerDataProvider.CreateClientFunc(true, tcpClient, reader);
                    client.ProtocolType = Shared.Enums.ProtocolType.Http;
                    string firstLine = reader.FirstLine;
                    string methodName = GetHttpMethodName(firstLine);
                    string address = GetHttpAddress(firstLine);
                        ((HttpClientInfo)client).RequestHeaders = reader.RequestHeaders;

                    await HandleHttpRequest(methodName, address, serverBase, (HttpClientInfo)client);
                }
                catch
                {
                    serverBase.DisposeClient(client, tcpClient, "HttpProvider StartToReadingClientData exception");
                }
                //}
            }
            catch (Exception ex)
            {
                //if (client != null)
                //serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase HttpProvider StartToReadingClientData");
                serverBase.DisposeClient(client, tcpClient, "HttpProvider StartToReadingClientData exception 2");
            }
        }


        /// <summary>
        /// Accept key for websoket client
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AcceptKey(ref string key)
        {
            string longKey = key + _guid;
            byte[] hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }

        private static SHA1 _sha1 = SHA1.Create();
        /// <summary>
        /// Compute sha1 hash
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static byte[] ComputeHash(string str)
        {
            return _sha1.ComputeHash(Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// get method name of http response
        /// </summary>
        /// <param name="reponse">response string</param>
        /// <returns>method name like "GET"</returns>
        private static string GetHttpMethodName(string reponse)
        {
            string[] lines = reponse.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
                return lines[0];
            return "";
        }

        /// <summary>
        /// get http address from response
        /// </summary>
        /// <param name="reponse">response string</param>
        /// <returns>address</returns>
        private static string GetHttpAddress(string reponse)
        {
            string[] lines = reponse.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1)
                return lines[1];
            return "";
        }

        //private bool IsMethodInfoOfJsonParameters(IEnumerable<MethodInfo> methods, List<string> names)
        //{
        //    bool isFind = false;
        //    foreach (MethodInfo method in methods)
        //    {
        //        int fakeParameterCount = 0;
        //        int findCount = method.GetCustomAttributes<FakeParameterAttribute>().Count();
        //        fakeParameterCount += findCount;
        //        if (method.GetParameters().Length == names.Count - fakeParameterCount)
        //        {
        //            for (int i = 0; i < fakeParameterCount; i++)
        //            {
        //                if (names.Count > 0)
        //                    names.RemoveAt(names.Count - 1);
        //            }
        //        }
        //        if (method.GetParameters().Count(x => names.Any(y => y.ToLower() == x.Name.ToLower())) == names.Count)
        //        {
        //            isFind = true;
        //            break;
        //        }
        //    }
        //    return isFind;
        //}


    }
}
