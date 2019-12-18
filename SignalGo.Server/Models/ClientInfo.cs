using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace SignalGo.Server.Models
{
    /// <summary>
    /// client protocol
    /// </summary>
    public enum ClientProtocolType : byte
    {
        /// <summary>
        /// unknown
        /// </summary>
        None = 0,
        /// <summary>
        /// http protocol
        /// </summary>
        Http = 1,
        /// <summary>
        /// signalgo duplex
        /// </summary>
        SignalGoDuplex = 2,
        /// <summary>
        /// one way protocol of signalgo
        /// </summary>
        SignalGoOneWay = 3,
        /// <summary>
        /// stream protocol
        /// </summary>
        SignalGoStream = 4,
        /// <summary>
        /// web socket protocol
        /// </summary>
        WebSocket = 5,
        /// <summary>
        /// http duplex client
        /// </summary>
        HttpDuplex = 6
    }

    /// <summary>
    /// information of tcp client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverBase"></param>
        public ClientInfo(ServerBase serverBase)
        {
            CurrentClientServer = serverBase;
        }
        /// <summary>
        /// current client server
        /// </summary>
        public ServerBase CurrentClientServer { get; set; }
        /// <summary>
        /// client id
        /// </summary>
        public string ClientId { get; set; }

        string _IPAddress;
        /// <summary>
        /// ip address of client
        /// </summary>
        public string IPAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_IPAddress))
                    _IPAddress = new System.Net.IPAddress(IPAddressBytes).ToString();
                return _IPAddress;
            }
        }

        byte[] _IPAddressBytes;
        /// <summary>
        /// bytes of ip address
        /// </summary>
        public byte[] IPAddressBytes
        {
            get
            {
                return _IPAddressBytes;
            }
            set
            {
                _IPAddressBytes = value;
                _IPAddress = null;
            }
        }

        /// <summary>
        /// version of client
        /// </summary>
        public uint? ClientVersion { get; set; }
        /// <summary>
        /// tag of client
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// when client disconnected
        /// </summary>
        public Action OnDisconnected { get; set; }
        /// <summary>
        /// tcp client
        /// </summary>
        internal TcpClient TcpClient { get; set; }
        /// <summary>
        /// date of connected
        /// </summary>
        public DateTime ConnectedDateTime { get; set; }
        /// <summary>
        /// stream of client to read and write
        /// </summary>
        public PipeNetworkStream ClientStream { get; set; }
        /// <summary>
        /// client Stream
        /// </summary>
        public ISignalGoStream StreamHelper { get; set; } = null;

        /// <summary>
        /// is client of owin client
        /// </summary>
        public virtual bool IsOwinClient
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// is websocket client
        /// </summary>
        public bool IsWebSocket { get; set; }
        /// <summary>
        /// type of client protocol
        /// </summary>
        public ClientProtocolType ProtocolType { get; set; } = ClientProtocolType.None;
        /// <summary>
        /// lock for this client
        /// </summary>
        public SemaphoreSlim LockWaitToRead { get; set; } = new SemaphoreSlim(1, 1);

    }

    /// <summary>
    /// information of http client
    /// </summary>
    public class HttpClientInfo : ClientInfo, IHttpClientInfo
    {
        /// <summary>
        /// current server base
        /// </summary>
        /// <param name="serverBase"></param>
        public HttpClientInfo(ServerBase serverBase) : base(serverBase)
        {
        }
        /// <summary>
        /// status of response for client
        /// </summary>
        public System.Net.HttpStatusCode Status { get; set; } = System.Net.HttpStatusCode.OK;
        /// <summary>
        /// headers of request that client sended
        /// </summary>
        public virtual IDictionary<string, string[]> RequestHeaders { get; set; }
        /// <summary>
        /// reponse headers to client
        /// </summary>
        public virtual IDictionary<string, string[]> ResponseHeaders { get; set; } = new WebHeaderCollection();
        /// <summary>
        /// key parameter value
        /// </summary>
        public string HttpKeyParameterValue { get; set; }
        /// <summary>
        /// file of http posted file
        /// </summary>
        private HttpPostedFileInfo _currentFile = null;
        public void SetFirstFile(HttpPostedFileInfo fileInfo)
        {
            _currentFile = fileInfo;
        }

        public HttpPostedFileInfo TakeNextFile()
        {
            return _currentFile;
        }

        public virtual string GetRequestHeaderValue(string header)
        {
            if (!RequestHeaders.ContainsKey(header))
                return null;
            return ((WebHeaderCollection)RequestHeaders)[header];
        }

        /// <summary>
        /// change status code before send data as headers etc
        /// </summary>
        /// <param name="statusCode"></param>
        public virtual void ChangeStatusCode(System.Net.HttpStatusCode statusCode)
        {
            Status = statusCode;
        }

    }

}
