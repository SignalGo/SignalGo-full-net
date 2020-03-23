// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using SignalGo.Server.Models;
using SignalGo.Shared.IO;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Controllers
{
    /// <summary>
    /// main structre of your server controller
    /// server controller has more fuction delegates it will override from developer plan
    /// the plan will remove no need ifs and remove empty codes and it will make signalgo very fast and powerful
    /// </summary>
    public abstract class ServerDataControllerBase
    {
        /// <summary>
        /// start server action
        /// </summary>
        public abstract void Start(ServerBase serverBase, int port);
        /// <summary>
        /// exchange and generate client function
        /// </summary>
        internal abstract Task ExchangeClient(ServerBase serverBase, PipeLineStream streamReader, TcpClient tcpClient);
        /// <summary>
        /// create client function
        /// </summary>
        public Func<ServerBase, ClientInfo, TcpClient, PipeLineStream, ClientInfo> CreateClientFunc { get; set; }
    }
}
