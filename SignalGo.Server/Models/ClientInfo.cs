﻿using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Http;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
        internal bool IsVerification { get; set; }
        internal ServerBase ServerBase { get; set; }
        internal SynchronizationContext MainContext { get; set; }
        internal Thread MainThread { get; set; }
        internal System.Net.IPEndPoint UdpIp { get; set; }
        internal bool IsWebSocket { get; set; }
        internal DateTime ConnectedDateTime { get; set; }
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
