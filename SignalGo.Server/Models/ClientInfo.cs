using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Net.Sockets;

namespace SignalGo.Server.Models
{
    /// <summary>
    /// information of tcp client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// client id
        /// </summary>
        public string ClientId { get; internal set; }
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

        internal TcpClient TcpClient { get; set; }
        internal DateTime ConnectedDateTime { get; set; }
        /// <summary>
        /// stream of client to read and write
        /// </summary>
        public PipeNetworkStream ClientStream { get; set; }
        /// <summary>
        /// client Stream
        /// </summary>
        internal ISignalGoStream StreamHelper { get; set; } = null;
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
        public WebHeaderCollection RequestHeaders { get; set; }
        /// <summary>
        /// reponse headers to client
        /// </summary>
        public WebHeaderCollection ResponseHeaders { get; set; } = new WebHeaderCollection();

        private HttpPostedFileInfo _currentFile = null;
        public void SetFirstFile(HttpPostedFileInfo fileInfo)
        {
            _currentFile = fileInfo;
        }

        public HttpPostedFileInfo TakeNextFile()
        {
            return _currentFile;
        }
    }

}
