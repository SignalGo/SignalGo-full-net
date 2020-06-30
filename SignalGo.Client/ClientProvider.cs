using Newtonsoft.Json;
using SignalGo.Client.ClientManager;
using SignalGo.Shared;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using SignalGo.Shared.Security;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Client
{
    /// <summary>
    /// provider for client to connect server and user calls and callbacks
    /// </summary>
    public class ClientProvider : UdpConnectorBase
    {
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="url">server url address</param>
        /// <param name="isWebsocket"></param>
        public override void Connect(string url)
        {
#if (!NETSTANDARD2_0 && !NET45)
            if (ProtocolType != ClientProtocolType.HttpDuplex)
                throw new NotSupportedException();
#endif
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new Exception("url is not valid");
            }
            else if (uri.Port <= 0)
            {
                throw new Exception("port is not valid");
            }

            if (uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("ws", StringComparison.OrdinalIgnoreCase))
                ProtocolType = ClientProtocolType.WebSocket;
            ServerUrl = url;
            string hostName = uri.Host;
            base.Connect(hostName, uri.Port);
#if (NET40 || NET35)
            SendFirstLineData();
#else
            SendFirstLineData().GetAwaiter().GetResult();
#endif
            GetClientIdIfNeed();

            IsConnected = true;
            RunPriorities();
            StartToReadingClientData();
            if (IsAutoReconnecting)
                OnConnectionChanged?.Invoke(ConnectionStatus.Reconnected);
            else
                OnConnectionChanged?.Invoke(ConnectionStatus.Connected);
        }

        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isWebsocket"></param>
        /// <returns></returns>
#if (!NET40 && !NET35)
        public override async Task ConnectAsync(string url)
        {
#if (!NETSTANDARD2_0 && !NET45)
            if (ProtocolType != ClientProtocolType.HttpDuplex)
                throw new NotSupportedException();
#endif

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new Exception("url is not valid");
            }
            else if (uri.Port <= 0)
            {
                throw new Exception("port is not valid");
            }
            if (uri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("ws", StringComparison.OrdinalIgnoreCase))
                ProtocolType = ClientProtocolType.WebSocket;
            ServerUrl = url;
            string hostName = uri.Host;
            await base.ConnectAsync(hostName, uri.Port);
            await SendFirstLineData();
            await GetClientIdIfNeedAsync();
            StartToReadingClientData();

            IsConnected = true;
            await RunPrioritiesAsync();
            if (IsAutoReconnecting)
                OnConnectionChanged?.Invoke(ConnectionStatus.Reconnected);
            else
                OnConnectionChanged?.Invoke(ConnectionStatus.Connected);
        }
#endif

        private readonly bool _oneTimeConnectedAsyncCalledWithAutoReconnect = false;

        private AutoResetEvent HoldThreadResetEvent { get; set; } = new AutoResetEvent(false);
        /// <summary>
        /// connect to server is background Thread
        /// </summary>
        /// <param name="url">url of server to connect</param>
        /// <param name="connectedAction">call this action after connect successfully</param>
        /// <param name="isAutoRecconect">if you want system try to reConnect when server or network is not avalable</param>
        /// <param name="isHoldMethodCallsWhenDisconnected">hold method calls when provider is disconnected and call all after connected</param>
        /// <param name="isWebsocket">is web socket system</param>
        //public void ConnectAsync(string url, Action<bool> connectedAction, bool isAutoRecconect, bool isHoldMethodCallsWhenDisconnected, bool isWebsocket = false)
        //{
        //    AsyncActions.Run(() =>
        //    {
        //        ProviderSetting.AutoReconnect = isAutoRecconect;
        //        ProviderSetting.HoldMethodCallsWhenDisconnected = isHoldMethodCallsWhenDisconnected;
        //        Connect(url, isWebsocket);
        //        connectedAction(true);
        //        HoldThreadResetEvent.Reset();
        //        HoldThreadResetEvent.WaitOne();
        //    }, (ex) =>
        //    {
        //        Disconnect();
        //        connectedAction(IsConnected);
        //        HoldThreadResetEvent.Reset();
        //        HoldThreadResetEvent.WaitOne();
        //    });
        //}

        /// <summary>
        /// connect to server is background Thread
        /// </summary>
        /// <param name="url">url of server to connect</param>
        /// <param name="connectedAction">call this action after connect successfully</param>
#if (NET35 || NET40)
        public void ConnectAsyncAutoReconnect(string url, Action<bool> connectedAction)
        {
            AsyncActions.Run(() =>
            {
                ProviderSetting.AutoReconnect = true;
                try
                {
                    ConnectAsync(url);
                    connectedAction(true);
                    AutoReconnectWaitToDisconnectTaskResult.Task.Wait();
                    AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();
                    ConnectAsyncAutoReconnect(url, connectedAction);
                }
                catch (Exception ex)
                {
                    connectedAction(false);
                    Disconnect();
                    AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();
                    ConnectAsyncAutoReconnect(url, connectedAction);
                }
            });
        }
#else
        public void ConnectAsyncAutoReconnect(string url, Action<bool> connectedAction)
        {
            Task.Run(async () =>
            {
                ProviderSetting.AutoReconnect = true;
                try
                {
                    await ConnectAsync(url);
                    try
                    {
                        connectedAction(true);
                    }
                    catch
                    {

                    }
                    await AutoReconnectWaitToDisconnectTaskResult.Task;
                    await Task.Delay(1000);
                    AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();

                    ConnectAsyncAutoReconnect(url, connectedAction);

                }
                catch (Exception ex)
                {
                    try
                    {
                        connectedAction(false);
                    }
                    catch
                    {

                    }
                    Disconnect();
                    await Task.Delay(1000);
                    AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();
                    ConnectAsyncAutoReconnect(url, connectedAction);
                }
            });
        }

        int retryAttempted = 0;

        /// <summary>
        /// Connect to server and reconnect if disconnected by specified timeout(for max retry)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="connectedAction"></param>
        /// <param name="retryAttempt">retry attempt/count</param>
        public void ConnectAsyncAutoReconnect(string url, Action<bool> connectedAction, int retryAttempt)
        {
            Task.Run(async () =>
            {
                ProviderSetting.AutoReconnect = true;
                while (retryAttempted <= retryAttempt)
                {
                    try
                    {
                        await ConnectAsync(url);
                        try
                        {
                            connectedAction(true);
                        }
                        catch
                        {

                        }
                        await AutoReconnectWaitToDisconnectTaskResult.Task;
                        await Task.Delay(1000);
                        AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();
                        retryAttempted++;
                    }
                    catch (Exception ex)
                    {
                        while (retryAttempted <= retryAttempt)
                        {
                            try
                            {
                                connectedAction(false);
                            }
                            catch
                            {

                            }
                            Disconnect();
                            await Task.Delay(1000);
                            AutoReconnectWaitToDisconnectTaskResult = new TaskCompletionSource<object>();
                            retryAttempted++;
                        }
                    }
                    finally
                    {
                        retryAttempted = 0;
                    }
                    break;
                }
            });
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securitySettings"></param>
#if (NET40 || NET35)
        public void SetSecuritySettings(SecuritySettingsInfo securitySettings)
#else
        public async void SetSecuritySettings(SecuritySettingsInfo securitySettings)
#endif
        {
            SecuritySettings = null;
            if (securitySettings.SecurityMode == SecurityMode.None)
            {
                securitySettings.Data = null;
#if (NET40 || NET35)
                SecuritySettingsInfo result = ConnectorExtensions.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
#else
                SecuritySettingsInfo result = await ConnectorExtensions.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
#endif
            }
            else if (securitySettings.SecurityMode == SecurityMode.RSA_AESSecurity)
            {
#if (!PORTABLE)
                RSAKey keys = RSASecurity.GenerateRandomKey();
                securitySettings.Data = new RSAAESEncryptionData() { RSAEncryptionKey = keys.PublicKey };
#if (NET40 || NET35)
                SecuritySettingsInfo result = ConnectorExtensions.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
#else
                SecuritySettingsInfo result = await ConnectorExtensions.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
#endif
                SecuritySettings = new SecuritySettingsInfo() { Data = new RSAAESEncryptionData() { Key = RSASecurity.Decrypt(result.Data.Key, RSASecurity.StringToKey(keys.PrivateKey)), IV = RSASecurity.Decrypt(result.Data.IV, RSASecurity.StringToKey(keys.PrivateKey)) }, SecurityMode = securitySettings.SecurityMode };
#endif
            }
        }

#if (NET40 || NET35)
        private void SendFirstLineData()
#else
        private Task SendFirstLineData()
#endif
        {
            byte[] firstBytes = null;
            if (ProtocolType == ClientProtocolType.HttpDuplex)
            {
                string newLine = TextHelper.NewLine;
                Uri.TryCreate(ServerUrl, UriKind.Absolute, out Uri uri);
                string port = uri.Port == 80 ? "" : ":" + uri.Port;
                string headData = $"POST {uri.AbsolutePath} HTTP/1.1{newLine}Host: {uri.Host + port}{newLine}Connection: keep-alive{newLine}Content-Length: {1024 * 1024}{newLine}SignalGo: SignalGoHttpDuplex{newLine + newLine}";
                firstBytes = Encoding.UTF8.GetBytes(headData);
                //                string newLine = TextHelper.NewLine;
                //                Uri.TryCreate(ServerUrl, UriKind.Absolute, out Uri uri);
                //                string port = uri.Port == 80 ? "" : ":" + uri.Port;
                //                //string headData = $"GET {uri.AbsolutePath} HTTP/1.1{newLine}Host: {uri.Host + port}{newLine}Connection: keep-alive{newLine}Sec-WebSocket-Key: x3JJHMbDL1EzLkh9GBhXDw=={newLine}Sec-WebSocket-Protocol: chat, superchat{newLine}Sec-WebSocket-Version: 13{newLine + newLine}";
                //                string headData = $@"GET / HTTP/1.1
                //Host: {uri.Host + port}
                //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0
                //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
                //Accept-Language: en-US,en;q=0.5
                //Accept-Encoding: gzip, deflate
                //Sec-WebSocket-Version: 13
                //Origin: null
                //SignalGo: SignalGoHttpDuplex
                //Sec-WebSocket-Extensions: permessage-deflate
                //Sec-WebSocket-Key: Kf1/4OQ83wCdLhsU8LkwqQ==
                //DNT: 1
                //Connection: keep-alive, Upgrade
                //Pragma: no-cache
                //Cache-Control: no-cache
                //Upgrade: websocket{newLine + newLine}";
                //                firstBytes = Encoding.UTF8.GetBytes(headData);
            }
            else if (ProtocolType == ClientProtocolType.WebSocket)
            {
                //do nothing
#if (NET40 || NET35)
                return;
#else
                return Task.FromResult<object>(null);
#endif
            }
            else
            {
                firstBytes = Encoding.UTF8.GetBytes($"SignalGo/4.0 {_address}:{_port}" + TextHelper.NewLine);
            }
#if (NET40 || NET35)
            StreamHelper.WriteToStream(_clientStream, firstBytes);
#else
            return StreamHelper.WriteToStreamAsync(_clientStream, firstBytes);
#endif
        }

#if (!NET40 && !NET35)
        private async Task GetClientIdIfNeedAsync()
        {
            if (ProviderSetting.AutoDetectRegisterServices && ProtocolType != ClientProtocolType.HttpDuplex && ProtocolType != ClientProtocolType.WebSocket)
            {
                byte[] data = new byte[]
                {
                    (byte)DataType.GetClientId,
                    (byte)CompressMode.None
                };

                await StreamHelper.WriteToStreamAsync(_clientStream, data.ToArray());
            }
        }
#endif

        private void GetClientIdIfNeed()
        {
            if (ProviderSetting.AutoDetectRegisterServices && ProtocolType != ClientProtocolType.HttpDuplex && ProtocolType != ClientProtocolType.WebSocket)
            {
                byte[] data = new byte[]
                {
                    (byte)DataType.GetClientId,
                    (byte)CompressMode.None
                };
                StreamHelper.WriteToStream(_clientStream, data.ToArray());
            }
        }
    }
}