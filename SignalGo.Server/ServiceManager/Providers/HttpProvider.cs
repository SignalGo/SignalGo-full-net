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
                    await HandleHttpRequest(methodName, address, serverBase, client).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (client.IsOwinClient)
                        throw;
                    serverBase.DisposeClient(client, null, "HttpProvider AddHttpClient exception");
                }
            }).ConfigureAwait(false);
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
                    await WebSocketProvider.StartToReadingClientData(client, serverBase).ConfigureAwait(false);
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
                    await SignalgoWebSocketProvider.StartToReadingClientData(client, serverBase).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    serverBase.DisposeClient(client, null, "HttpProvider AddWebSocketHttpClient exception");
                }
            });
        }

        static readonly char[] splitChars = { ':' };
        public static async Task StartToReadingClientData(TcpClient tcpClient, ServerBase serverBase, PipeNetworkStream reader, StringBuilder builder)
        {
            //Console.WriteLine($"Http Client Connected: {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString().Replace("::ffff:", "")}");
            ClientInfo client = null;
            try
            {
                reader.MaximumLineSize = serverBase.ProviderSetting.HttpSetting.MaximumHeaderSize;
                if (reader.MaximumLineSize > 0)
                {
                    reader.MaximumLineSizeReadedFunction = () =>
                    {
                        return serverBase.Firewall.OnDangerDataReceived(tcpClient, client, Firewall.DangerDataType.HeaderSize);
                    };
                }

                Dictionary<string, string> headers = new Dictionary<string, string>();
                string firstLine = builder.ToString();
                while (true)
                {
                    string line = await reader.ReadLineAsync().ConfigureAwait(false);
                    builder.Append(line);

                    if (line == TextHelper.NewLine)
                        break;
                    var split = line.Split(splitChars, 2);
                    if (split.Length != 2)
                    {
                        if (!await serverBase.Firewall.OnDangerDataReceived(tcpClient, client, Firewall.DangerDataType.InvalidHeader).ConfigureAwait(false))
                        {
                            serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                            return;
                        }
                    }
                    string headerKey = split[0];
                    string headerValue = split[1];

                    if (!headers.ContainsKey(headerKey))
                    {
                        if (!serverBase.Firewall.OnHttpHeaderComepleted(tcpClient, ref headerKey, ref headerValue))
                        {
                            serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                            return;
                        }
                        headers.Add(headerKey, headerValue);
                    }
                }


                if (headers.ContainsKey("Sec-WebSocket-Key"))
                {
                    string requestHeaders = builder.ToString();
                    tcpClient.ReceiveTimeout = -1;
                    tcpClient.SendTimeout = -1;
                    client = serverBase.ServerDataProvider.CreateClientInfo(false, tcpClient, reader);
                    client.ProtocolType = ClientProtocolType.WebSocket;
                    client.IsWebSocket = true;
                    client.LevelFlag = "WebSocket_LF";
                    if (!await serverBase.Firewall.OnClientInitialized(client).ConfigureAwait(false))
                    {
                        serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                        return;
                    }

                    string key = requestHeaders.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
                    string acceptKey = AcceptKey(ref key);
                    string newLine = TextHelper.NewLine;

                    var response = "HTTP/1.1 101 Switching Protocols" + newLine
                    //string response = "HTTP/1.0 101 Switching Protocols" + newLine
                     + "Upgrade: websocket" + newLine
                     + "Connection: Upgrade" + newLine
                     + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(response);
                    await client.ClientStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    client.ClientStream = new PipeNetworkStream(new WebSocketStream(client.TcpClient.GetStream()));
                    await AddWebSocketHttpClient(client, serverBase).ConfigureAwait(false);
                    //if (requestHeaders.Contains("SignalgoDuplexWebSocket"))
                    //{
                    //    await WebSocketProvider.StartToReadingClientData(client, serverBase);
                    //}
                    //else
                    //{
                    //    await WebSocketProvider.StartToReadingClientData(client, serverBase);
                    //    //client.StreamHelper = SignalGoStreamWebSocketLlight.CurrentWebSocket;
                    //    //client.ClientStream = new PipeNetworkStream(new WebSocketStream(client.TcpClient.GetStream()));
                    //    //await WebSocketProvider.StartToReadingClientData(client, serverBase);

                    //    //await HttpProvider.AddWebSocketHttpClient(client, serverBase);
                    //}
                }
                else if (headers.ContainsKey("SignalGoHttpDuplex"))
                {
                    client = serverBase.ServerDataProvider.CreateClientInfo(false, tcpClient, reader);
                    client.ProtocolType = ClientProtocolType.HttpDuplex;
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    client.LevelFlag = "SGHttpDuplex_LF";
                    if (!await serverBase.Firewall.OnClientInitialized(client).ConfigureAwait(false))
                    {
                        serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                        return;
                    }
                    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase).ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        //serverBase.TaskOfClientInfoes
                        client = (HttpClientInfo)serverBase.ServerDataProvider.CreateClientInfo(true, tcpClient, reader);
                        client.LevelFlag = "Http_LF";
                        client.ProtocolType = ClientProtocolType.Http;
                        client.StreamHelper = SignalGoStreamBase.CurrentBase;
                        if (!await serverBase.Firewall.OnClientInitialized(client).ConfigureAwait(false))
                        {
                            serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                            return;
                        }
                        //string[] lines = null;
                        //if (requestHeaders.Contains(TextHelper.NewLine + TextHelper.NewLine))
                        //    lines = requestHeaders.Substring(0, requestHeaders.IndexOf(TextHelper.NewLine + TextHelper.NewLine)).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        //else
                        //    lines = requestHeaders.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (headers.Count > 0)
                        {
                            string methodName = GetHttpMethodName(firstLine);
                            string address = GetHttpAddress(firstLine);
                            ((HttpClientInfo)client).RequestHeaders = SignalGo.Shared.Http.WebHeaderCollection.GetHttpHeaders(headers);
                            if (!await serverBase.Firewall.OnHttpHeadersComepleted(client).ConfigureAwait(false))
                            {
                                serverBase.DisposeClient(client, tcpClient, "firewall dropped!");
                                return;
                            }
                            await HandleHttpRequest(methodName, address, serverBase, (HttpClientInfo)client).ConfigureAwait(false);
                        }
                        else
                            serverBase.DisposeClient(client, tcpClient, "HttpProvider StartToReadingClientData no line detected");
                    }
                    catch
                    {
                        serverBase.DisposeClient(client, tcpClient, "HttpProvider StartToReadingClientData exception");
                    }
                }
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

        private bool IsMethodInfoOfJsonParameters(IEnumerable<MethodInfo> methods, List<string> names)
        {
            bool isFind = false;
            foreach (MethodInfo method in methods)
            {
                int fakeParameterCount = 0;
                int findCount = method.GetCustomAttributes<FakeParameterAttribute>().Count();
                fakeParameterCount += findCount;
                if (method.GetParameters().Length == names.Count - fakeParameterCount)
                {
                    for (int i = 0; i < fakeParameterCount; i++)
                    {
                        if (names.Count > 0)
                            names.RemoveAt(names.Count - 1);
                    }
                }
                if (method.GetParameters().Count(x => names.Any(y => y.ToLower() == x.Name.ToLower())) == names.Count)
                {
                    isFind = true;
                    break;
                }
            }
            return isFind;
        }


    }
}
