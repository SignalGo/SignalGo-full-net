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
        public override void Connect(string url, bool isWebsocket = false)
        {
            IsWebSocket = isWebsocket;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new Exception("url is not valid");
            }
            else if (uri.Port <= 0)
            {
                throw new Exception("port is not valid");
            }
            ServerUrl = url;
            string hostName = uri.Host;
            base.Connect(hostName, uri.Port);
            SendFirstLineData();
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
        public override async Task ConnectAsync(string url, bool isWebsocket = false)
        {
            IsWebSocket = isWebsocket;
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                throw new Exception("url is not valid");
            }
            else if (uri.Port <= 0)
            {
                throw new Exception("port is not valid");
            }
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
                    connectedAction(true);
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
            byte[] firstBytes = Encoding.UTF8.GetBytes($"SignalGo/4.0 {_address}:{_port}" + TextHelper.NewLine);
#if (NET40 || NET35)
            _client.GetStream().Write(firstBytes, 0, firstBytes.Length);
#else
            return _client.GetStream().WriteAsync(firstBytes, 0, firstBytes.Length);
#endif
        }

#if (!NET40 && !NET35)
        private async Task GetClientIdIfNeedAsync()
        {
            if (ProviderSetting.AutoDetectRegisterServices)
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
            if (ProviderSetting.AutoDetectRegisterServices)
            {
                byte[] data = new byte[]
                {
                    (byte)DataType.GetClientId,
                    (byte)CompressMode.None
                };
                
                StreamHelper.WriteToStream(_clientStream, data.ToArray());
            }
        }

        public void TestDisConnect()
        {
#if (!NETSTANDARD1_6)
            _client.Close();
#endif
        }
    }
}