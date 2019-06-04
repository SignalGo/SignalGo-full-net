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

namespace SignalGo.Server.ServiceManager.Versions
{
    /// <summary>
    /// main structre of your server provider
    /// server provider has more fuction deleates it will ovverride from developer plan
    /// the plan will remove no need ifs and remove empty codes and it will make signalgo very fast and powerful
    /// </summary>
    public abstract class ServerDataProviderBase
    {
        /// <summary>
        /// start server action
        /// </summary>
        public Action<ServerBase, int> StartAction { get; set; }
        /// <summary>
        /// exchange and generate client function
        /// </summary>
        public Func<ServerBase, PipeNetworkStream, TcpClient, Task> ExchangeClientFunc { get; set; }
        /// <summary>
        /// create client function
        /// </summary>
        public Func<ServerBase, ClientInfo, TcpClient, PipeNetworkStream, ClientInfo> CreateClientFunc { get; set; }
    }
}
