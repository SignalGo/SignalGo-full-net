using Newtonsoft.Json;
using SignalGo.Client.ClientManager;
using SignalGo.Shared;
using SignalGo.Shared.Models;
using SignalGo.Shared.Security;
using System;
using System.Linq;
using System.Text;
using System.Threading;

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
            //            if (Uri.CheckHostName(uri.Host) == UriHostNameType.IPv4 || Uri.CheckHostName(uri.Host) == UriHostNameType.IPv6)
            //            {
            //                Host = uri.Host;
            //            }
            //            else
            //            {
            //#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            //                var addresses = Dns.GetHostEntryAsync(uri.Host).Result;
            //                Host = addresses.AddressList.Length == 0 ? uri.Host : addresses.AddressList.FirstOrDefault().ToString();
            //#elif (PORTABLE)
            //                // Bind to a Domain Name Server
            //                DNS.Client.DnsClient client = new DNS.Client.DnsClient("8.8.8.8");

            //                // Create request bound to 8.8.8.8
            //                DNS.Client.ClientRequest request = client.Create();

            //                // Returns a list of IPs
            //                IList<DNS.Protocol.IPAddress> ips = client.Lookup(uri.Host).Result;
            //                Host = ips.FirstOrDefault().ToString();
            //#else
            //                var addresses = Dns.GetHostEntry(uri.Host);
            //                Host = addresses.AddressList.Length == 0 ? uri.Host : addresses.AddressList.FirstOrDefault().ToString();
            //#endif
            //            }

            //IPHostEntry server = Dns.Resolve(uri.Host);
#if (PORTABLE)
            base.Connect(hostName, uri.Port);

#else
            base.Connect(hostName, uri.Port);
#endif
            SendFirstLineData();
            GetClientIdIfNeed();
            StartToReadingClientData();

            IsConnected = true;
            RunPriorities();
            if (IsAutoReconnecting)
                OnConnectionChanged?.Invoke(ConnectionStatus.Reconnected);
            else
                OnConnectionChanged?.Invoke(ConnectionStatus.Connected);
        }

        private bool _oneTimeConnectedAsyncCalledWithAutoReconnect = false;

        private AutoResetEvent HoldThreadResetEvent { get; set; } = new AutoResetEvent(false);
        /// <summary>
        /// connect to server is background Thread
        /// </summary>
        /// <param name="url">url of server to connect</param>
        /// <param name="connectedAction">call this action after connect successfully</param>
        /// <param name="isAutoRecconect">if you want system try to reConnect when server or network is not avalable</param>
        /// <param name="isHoldMethodCallsWhenDisconnected">hold method calls when provider is disconnected and call all after connected</param>
        /// <param name="isWebsocket">is web socket system</param>
        public void ConnectAsync(string url, Action<bool> connectedAction, bool isAutoRecconect, bool isHoldMethodCallsWhenDisconnected, bool isWebsocket = false)
        {
            AsyncActions.Run(() =>
            {
                ProviderSetting.AutoReconnect = isAutoRecconect;
                ProviderSetting.HoldMethodCallsWhenDisconnected = isHoldMethodCallsWhenDisconnected;
                Connect(url, isWebsocket);
                connectedAction(true);
                HoldThreadResetEvent.Reset();
                HoldThreadResetEvent.WaitOne();
            }, (ex) =>
            {
                Disconnect();
                connectedAction(IsConnected);
                HoldThreadResetEvent.Reset();
                HoldThreadResetEvent.WaitOne();
            });
        }

        private readonly object _connectAsyncAutoReconnectLock = new object();
        /// <summary>
        /// connect to server is background Thread
        /// </summary>
        /// <param name="url">url of server to connect</param>
        /// <param name="connectedAction">call this action after connect successfully</param>
        /// <param name="isWebsocket">is web socket system</param>
        public void ConnectAsyncAutoReconnect(string url, Action<bool> connectedAction, bool isWebsocket = false)
        {
            lock (_connectAsyncAutoReconnectLock)
            {
                if (_oneTimeConnectedAsyncCalledWithAutoReconnect)
                {
                    AutoReconnectDelayResetEvent.Set();
                }
                else
                {
                    _oneTimeConnectedAsyncCalledWithAutoReconnect = true;
                    ConnectAsync(url, connectedAction, true, true, isWebsocket);
                }
            }
        }

        public void TryAutoReconnect()
        {
            AutoReconnectDelayResetEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securitySettings"></param>
        public void SetSecuritySettings(SecuritySettingsInfo securitySettings)
        {
            SecuritySettings = null;
            if (securitySettings.SecurityMode == SecurityMode.None)
            {
                securitySettings.Data = null;
                SecuritySettingsInfo result = ConnectorExtensions.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });

            }
            else if (securitySettings.SecurityMode == SecurityMode.RSA_AESSecurity)
            {
#if (!PORTABLE)
                RSAKey keys = RSASecurity.GenerateRandomKey();
                securitySettings.Data = new RSAAESEncryptionData() { RSAEncryptionKey = keys.PublicKey };
                SecuritySettingsInfo result = ConnectorExtensions.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
                SecuritySettings = new SecuritySettingsInfo() { Data = new RSAAESEncryptionData() { Key = RSASecurity.Decrypt(result.Data.Key, RSASecurity.StringToKey(keys.PrivateKey)), IV = RSASecurity.Decrypt(result.Data.IV, RSASecurity.StringToKey(keys.PrivateKey)) }, SecurityMode = securitySettings.SecurityMode };
#endif
            }
        }

        private void SendFirstLineData()
        {
            byte[] firstBytes = Encoding.UTF8.GetBytes($"SignalGo/4.0 {_address}:{_port}" + "\r\n");
            _client.GetStream().Write(firstBytes, 0, firstBytes.Length);
        }

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
    }
}