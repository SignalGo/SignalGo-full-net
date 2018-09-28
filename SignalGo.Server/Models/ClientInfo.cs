using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SignalGo.Server.Models
{
    public enum ClientProtocolType : byte
    {
        None = 0,
        Http = 1,
        SignalGoDuplex = 2,
        SignalGoOneWay = 3,
        SignalGoStream = 4,
        WebSocket = 5,
    }

    /// <summary>
    /// information of tcp client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// client id
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// ip address of client
        /// </summary>
        public string IPAddress { get; set; }
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

        public virtual bool IsOwinClient
        {
            get
            {
                return false;
            }
        }

        public bool IsWebSocket { get; set; }

        public ClientProtocolType ProtocolType { get; set; } = ClientProtocolType.None;
    }

    /// <summary>
    /// information of http client
    /// </summary>
    public class HttpClientInfo : ClientInfo, IHttpClientInfo
    {
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
    }

}
