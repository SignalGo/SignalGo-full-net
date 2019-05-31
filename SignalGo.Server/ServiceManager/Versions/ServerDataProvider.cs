// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SignalGo.Server.Models;
using SignalGo.Shared.IO;

namespace SignalGo.Server.ServiceManager.Versions
{
    /// <summary>
    /// main server provider
    /// its for check timeouts for clients
    /// if you dont set it,server dont need to check it always and it will have better performance
    /// </summary>
    public class ServerDataProvider : ServerDataProviderBase
    {
        /// <summary>
        /// call on client connected action
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <param name="tcpClient"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override ClientInfo CreateClientInfo(ServerBase serverBase, ClientInfo client, TcpClient tcpClient, PipeNetworkStream stream)
        {
            var result = base.CreateClientInfo(serverBase, client, tcpClient, stream);
            serverBase.OnClientConnectedAction?.Invoke(client);
            return result;
        }
        /// <summary>
        /// set client streams timeouts
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="streamReader"></param>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public override Task ExchangeClient(ServerBase serverBase, PipeNetworkStream streamReader, TcpClient tcpClient)
        {
            var result =  base.ExchangeClient(serverBase, streamReader, tcpClient);
            //set client timeouts
            if (serverBase.ProviderSetting.IsEnabledToUseTimeout)
            {
                tcpClient.GetStream().ReadTimeout = (int)serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                tcpClient.GetStream().WriteTimeout = (int)serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
            }
            return result;
        }
    }
}
