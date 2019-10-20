using SignalGo.Server.ServiceManager;
using SignalGo.Shared.Enums;
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SignalGo.Server.Models
{
    /// <summary>
    /// information of tcp client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="tcpClient"></param>
        public ClientInfo(ServerBase serverBase, System.Net.Sockets.TcpClient tcpClient)
        {
            CurrentClientServer = serverBase;
            TcpClient = tcpClient;
        }
        /// <summary>
        /// current client server
        /// </summary>
        public ServerBase CurrentClientServer { get; set; }
        /// <summary>
        /// client id
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// ip address of client
        /// </summary>
        public string IPAddress
        {
            get
            {
                return new System.Net.IPAddress(IPAddressBytes).ToString();
            }
        }

        /// <summary>
        /// bytes of ip address
        /// </summary>
        public byte[] IPAddressBytes
        {
            get
            {
                return ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address.GetAddressBytes();
            }
        }

        /// <summary>
        /// version of client
        /// </summary>
        public uint? ClientVersion { get; set; }
        /// <summary>
        /// when client disconnected
        /// </summary>
        public Action OnDisconnected { get; set; }
        /// <summary>
        /// tcp client
        /// </summary>
        internal System.Net.Sockets.TcpClient TcpClient { get; set; }
        /// <summary>
        /// date of connected
        /// </summary>
        public DateTime ConnectedDateTime { get; set; }
        /// <summary>
        /// stream of client to read and write
        /// </summary>
        public PipeLineStream ClientStream { get; set; }
        ///// <summary>
        ///// client Stream
        ///// </summary>
        //public ISignalGoStream StreamHelper { get; set; } = null;

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
        public ProtocolType ProtocolType { get; set; } = ProtocolType.None;
        /// <summary>
        /// lock for this client
        /// </summary>
        public SemaphoreSlim LockWaitToRead { get; set; } = new SemaphoreSlim(1, 1);

    }

    /// <summary>
    /// information of http client
    /// </summary>
    public class HttpClientInfo : ClientInfo
    {
        /// <summary>
        /// current server base
        /// </summary>
        /// <param name="serverBase"></param>
        public HttpClientInfo(ServerBase serverBase) : base(serverBase, null)
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
        public virtual IDictionary<string, string[]> ResponseHeaders { get; set; } //= new WebHeaderCollection();
        /// <summary>
        /// key parameter value
        /// </summary>
        public string HttpKeyParameterValue { get; set; }
        /// <summary>
        /// file of http posted file
        /// </summary>
        //private HttpPostedFileInfo _currentFile = null;
        //public void SetFirstFile(HttpPostedFileInfo fileInfo)
        //{
        //    _currentFile = fileInfo;
        //}

        //public HttpPostedFileInfo TakeNextFile()
        //{
        //    return _currentFile;
        //}

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

        }

    }

}
