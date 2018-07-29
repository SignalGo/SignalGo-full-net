using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Http;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SignalGo.Server.Models
{
    /// <summary>
    /// Informations about tcp client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// The client ID
        /// </summary>
        public string ClientId { get; internal set; }
        /// <summary>
        /// The client's ip address
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// The client version 
        /// </summary>
        public uint? ClientVersion { get; set; }
        /// <summary>
        /// General purpose client Tag property
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// Action raised after a client disconnects from server
        /// </summary>
        public Action OnDisconnected { get; set; }

        internal TcpClient TcpClient { get; set; }
        internal bool IsVerification { get; set; }
        internal ServerBase ServerBase { get; set; }
        internal SynchronizationContext MainContext { get; set; }
        internal Thread MainThread { get; set; }
        internal System.Net.IPEndPoint UdpIp { get; set; }
        internal bool IsWebSocket { get; set; }
        internal DateTime ConnectedDateTime { get; set; }

        public Stream ClientStream { get; set; }
    }

    /// <summary>
    /// informations about HTTP client
    /// </summary>
    public class HttpClientInfo : ClientInfo, IHttpClientInfo
    {
        /// <summary>
        /// Response status for client
        /// </summary>
        public System.Net.HttpStatusCode Status { get; set; } = System.Net.HttpStatusCode.OK;
        /// <summary>
        /// The request headers sent by the client
        /// </summary>
        public WebHeaderCollection RequestHeaders { get; set; }
        /// <summary>
        /// The reponse headers received by the client
        /// </summary>
        public WebHeaderCollection ResponseHeaders { get; set; } = new WebHeaderCollection();

        HttpPostedFileInfo _currentFile = null;
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
