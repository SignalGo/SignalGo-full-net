using Newtonsoft.Json;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using SignalGo.Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SignalGo.Client
{
    /// <summary>
    /// provider for client to connect server and user calls and callbacks
    /// </summary>
    public class ClientProvider : UdpConnectorBase
    {
        static ClientProvider()
        {
            JsonSettingHelper.Initialize();
        }
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="url">server url address</param>
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
        public async void Connect(string url, bool isWebsocket = false)
#else
        public void Connect(string url, bool isWebsocket = false)
#endif
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

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            IPHostEntry Host = await Dns.GetHostEntryAsync(uri.Host);
#else
            IPHostEntry Host = Dns.GetHostEntry(uri.Host);
#endif
            //IPHostEntry server = Dns.Resolve(uri.Host);
            base.Connect(Host.AddressList.Length == 0 ? uri.Host : Host.AddressList.FirstOrDefault().ToString(), uri.Port);
            Connect();
            ConnectToUrl(uri.AbsolutePath);
            StartToReadingClientData();
            var isConnected = ConnectorExtension.SendData<bool>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/CheckConnection" });
            Console.WriteLine("isConnected " + isConnected);
            if (!isConnected)
            {
                Dispose();
                throw new Exception("server is available but connection address is not true");
            }
        }

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
                var keys = RSASecurity.GenerateRandomKey();
                securitySettings.Data = new RSAAESEncryptionData() { RSAEncryptionKey = keys.PublicKey };
                var result = ConnectorExtension.SendData<SecuritySettingsInfo>(this, new Shared.Models.MethodCallInfo() { Guid = Guid.NewGuid().ToString(), ServiceName = "/SetSettings", Data = JsonConvert.SerializeObject(securitySettings) });
                SecuritySettings = new SecuritySettingsInfo() { Data = new RSAAESEncryptionData() { Key = RSASecurity.Decrypt(result.Data.Key, RSASecurity.StringToKey(keys.PrivateKey)), IV = RSASecurity.Decrypt(result.Data.IV, RSASecurity.StringToKey(keys.PrivateKey)) }, SecurityMode = securitySettings.SecurityMode };
            }
        }

        public void SetSetting(SettingInfo settingInfo)
        {
            SettingInfo = settingInfo;
        }

        void Connect()
        {
            var bbb = Encoding.UTF8.GetBytes("SignalGo/1.0" + System.Environment.NewLine);
            var len = _client.Client.Send(bbb);
            byte b1 = (byte)_client.GetStream().ReadByte();
            byte b2 = (byte)_client.GetStream().ReadByte();

            Console.WriteLine("Connect Write " + Encoding.UTF8.GetString(new byte[2] { b1, b2 }));
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
            Console.WriteLine("write url:" + bytes.Count);
            GoStreamWriter.WriteToStream(_client.GetStream(), bytes.ToArray(), IsWebSocket);
            Console.WriteLine("write complete:" + bytes.Count);
        }
    }
}