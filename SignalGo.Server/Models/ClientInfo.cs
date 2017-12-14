using SignalGo.Server.ServiceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SignalGo.Server.Models
{
    public class ClientInfo
    {
        public string ClientId { get; internal set; }
        public string IPAddress { get; internal set; }
        public uint? ClientVersion { get; set; }
        public object Tag { get; set; }

        public Action OnDisconnected { get; set; }

        internal TcpClient TcpClient { get; set; }
        internal bool IsVerification { get; set; }
        internal ServerBase ServerBase { get; set; }
        internal SynchronizationContext MainContext { get; set; }
        internal Thread MainThread { get; set; }
        internal IPEndPoint UdpIp { get; set; }
        internal bool IsWebSocket { get; set; }
        internal DateTime ConnectedDateTime { get; set; }
    }
}
