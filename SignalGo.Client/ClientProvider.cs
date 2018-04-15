using Newtonsoft.Json;
using SignalGo.Shared;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using SignalGo.Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            Connect();
            ConnectToUrl(uri.AbsolutePath);
            GetClientIdIfNeed();
            StartToReadingClientData();
            var isConnected = ConnectorExtension.SendData<bool>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/CheckConnection" });
//#if (!PORTABLE)
//            Console.WriteLine("isConnected " + isConnected);
//#endif
            if (!isConnected && !ProviderSetting.AutoDetectRegisterServices)
            {
                Disconnect();
                throw new Exception("server is available but connection address is not true");
            }
            IsConnected = true;
            RunPriorities();
            if (IsAutoReconnecting)
                OnConnectionChanged?.Invoke(ConnectionStatus.Reconnected);
            else
                OnConnectionChanged?.Invoke(ConnectionStatus.Connected);
        }

        volatile bool _oneTimeConnectedAsyncCalledWithAutoReconnect = false;
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
            }, (ex) =>
            {
                Disconnect();
                connectedAction(false);
            });
        }

        object _connectAsyncAutoReconnectLock = new object();
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
                var result = ConnectorExtension.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });

            }
            else if (securitySettings.SecurityMode == SecurityMode.RSA_AESSecurity)
            {
#if (!PORTABLE)
                var keys = RSASecurity.GenerateRandomKey();
                securitySettings.Data = new RSAAESEncryptionData() { RSAEncryptionKey = keys.PublicKey };
                var result = ConnectorExtension.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
                SecuritySettings = new SecuritySettingsInfo() { Data = new RSAAESEncryptionData() { Key = RSASecurity.Decrypt(result.Data.Key, RSASecurity.StringToKey(keys.PrivateKey)), IV = RSASecurity.Decrypt(result.Data.IV, RSASecurity.StringToKey(keys.PrivateKey)) }, SecurityMode = securitySettings.SecurityMode };
#endif
            }
        }

        void Connect()
        {
            try
            {
                var firstBytes = Encoding.UTF8.GetBytes("SignalGo/1.0" + System.Environment.NewLine);
#if (!PORTABLE)
                var len = _client.Client.Send(firstBytes);
                var stream = _client.GetStream();

#else
                _client.WriteStream.Write(firstBytes, 0, firstBytes.Length);
                var stream = _client.ReadStream;
#endif
                byte b1 = (byte)stream.ReadByte();
                byte b2 = (byte)stream.ReadByte();
//#if (!PORTABLE)
//                Console.WriteLine("Connect Write " + Encoding.UTF8.GetString(new byte[2] { b1, b2 }));
//#endif
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// send data to server for accept reality connection
        /// </summary>
        /// <param name="url"></param>
        void ConnectToUrl(string url)
        {
            var json = JsonConvert.SerializeObject(new List<string>() { url });
            List<byte> bytes = new List<byte>();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] dataLen = BitConverter.GetBytes(jsonBytes.Length);
            bytes.AddRange(dataLen);
            bytes.AddRange(jsonBytes);
//#if (!PORTABLE)
//            Console.WriteLine("write url:" + bytes.Count);
//#endif
#if (PORTABLE)
            GoStreamWriter.WriteToStream(_client.WriteStream, bytes.ToArray(), IsWebSocket);
#else
            GoStreamWriter.WriteToStream(_client.GetStream(), bytes.ToArray(), IsWebSocket);
#endif
//#if (!PORTABLE)
//            Console.WriteLine("write complete:" + bytes.Count);
//#endif
        }

        void GetClientIdIfNeed()
        {
            if (ProviderSetting.AutoDetectRegisterServices)
            {
                var data = new byte[]
                {
                    (byte)DataType.GetClientId,
                    (byte)CompressMode.None
                };
#if (PORTABLE)
                var stream = _client.WriteStream;
#else
                var stream = _client.GetStream();
#endif
                GoStreamWriter.WriteToStream(stream, data.ToArray(), IsWebSocket);
            }
        }
    }
}