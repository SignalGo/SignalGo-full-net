using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Models
{
    public class BaseSetting
    {
        /// <summary>
        /// if http protocolsetting is https
        /// </summary>
        public bool IsHttps { get; set; }
        /// <summary>
        /// when you want to use timeouts on your provider set it true
        /// </summary>
        public bool IsEnabledToUseTimeout { get; set; }
        /// <summary>
        /// maximum value of timeout to wait for send data
        /// </summary>
        public TimeSpan SendDataTimeout { get; set; } = new TimeSpan(0, 0, 30);
        /// <summary>
        /// maximum value of timeout to wait for receive callbackinfo data
        /// </summary>
        public TimeSpan ReceiveDataTimeout { get; set; } = new TimeSpan(0, 0, 30);
    }

    public class HttpSetting : BaseSetting
    {
        /// <summary>
        /// handle cross origin access from browser origin header
        /// </summary>
        public bool HandleCrossOriginAccess { get; set; }

        /// <summary>
        /// X509Certificate
        /// </summary>
        public System.Security.Cryptography.X509Certificates.X509Certificate X509Certificate { get; set; }
    }

    /// <summary>
    /// server or client connector provider setting
    /// </summary>
    public class ProviderSetting
    {
        public HttpSetting HttpSetting { get; set; } = new HttpSetting();
        public BaseSetting ServerServiceSetting { get; set; } = new BaseSetting();

        /// <summary>
        /// when you want to use timeouts on your provider set it true
        /// the properties of set timeout is SendDataTimeout and ReceiveDataTimeout
        /// or you can set it to false and server will wait fo data for ever
        /// default is false
        /// </summary>
        public bool IsEnabledToUseTimeout { get; set; }
        /// <summary>
        /// maximum value of timeout to wait for send data
        /// </summary>
        public TimeSpan SendDataTimeout { get; set; } = new TimeSpan(0, 0, 30);
        /// <summary>
        /// maximum value of timeout to wait for receive callbackinfo data
        /// </summary>
        public TimeSpan ReceiveDataTimeout { get; set; } = new TimeSpan(0, 0, 30);
        /// <summary>
        /// maximum send data block
        /// </summary>
        public uint MaximumSendDataBlock { get; set; } = uint.MaxValue;
        /// <summary>
        /// maximum receive data block
        /// </summary>
        public uint MaximumReceiveDataBlock { get; set; } = uint.MaxValue;
        /// <summary>
        /// maximum header of stream for download from client
        /// </summary>
        public uint MaximumReceiveStreamHeaderBlock { get; set; } = 65536;
        /// <summary>
        /// automatic try to reconnect after disconnect
        /// </summary>
        public bool AutoReconnect { get; set; }
        /// <summary>
        /// millisecound of wait for reconnect system
        /// </summary>
        public int AutoReconnectTime { get; set; } = 1000;

        /// <summary>
        /// hold method calls when provider is disconnected and call all after connected
        /// </summary>
        public bool HoldMethodCallsWhenDisconnected { get; set; }
        /// <summary>
        /// if true, when client get timeout error when calling server method client force diconnect from signalgo
        /// </summary>
        public bool DisconnectClientWhenTimeout { get; set; }
        /// <summary>
        /// Auto detect register services without send and receive /RegisterService to server and get accept
        /// </summary>
        public bool AutoDetectRegisterServices { get; set; } = true;
        /// <summary>
        /// call again priority func<bool> for get return true
        /// </summary>
        public int PriorityFunctionDelayTime { get; set; } = 2000;

        /// <summary>
        /// data exchanger is limitation of data types and properties to send and receive from client and server
        /// </summary>
        public bool IsEnabledDataExchanger { get; set; } = true;
        /// <summary>
        /// enable $Id 
        /// </summary>
        public bool IsEnabledReferenceResolver { get; set; } = true;
        /// <summary>
        ///  enable $ref 
        /// </summary>
        public bool IsEnabledReferenceResolverForArray { get; set; } = true;
        /// <summary>
        /// http attributes
        /// </summary>
        public List<HttpKeyAttribute> HttpKeyResponses { get; set; }
    }
}
