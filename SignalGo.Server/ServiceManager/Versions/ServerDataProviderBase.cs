// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori

using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.Converters;
using SignalGo.Shared.IO;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Versions
{
    /// <summary>
    /// server data provider communication between client and server
    /// version 4 of signalgo protocol
    /// </summary>
    public class ServerDataProviderBase : IServerDataProvider
    {
        /// <summary>
        /// server tcp listener waiting for client come from tcp
        /// </summary>
        internal TcpListener _server;
        /// <summary>
        /// if this provider is disposed
        /// </summary>
        internal bool IsDispose { get; set; } = false;
        /// <summary>
        /// start the server prtocol listener in this stage
        /// </summary>
        /// <param name="serverBase">what is your server base comming from</param>
        /// <param name="port">port number to listen</param>
        public virtual void Start(ServerBase serverBase, int port)
        {
            //run listener in a new thread
            Thread thread = new Thread(Started)
            {
                IsBackground = false
            };
            thread.Start();

            //start server listener
            void Started()
            {
                try
                {
                    _server = new TcpListener(IPAddress.IPv6Any, port);
                    _server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                    _server.Server.NoDelay = true;
                    _server.Start();

                    //set time outs when developer enable it in provider serrings
                    if (serverBase.ProviderSetting.IsEnabledToUseTimeout)
                    {
                        _server.Server.SendTimeout = (int)serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
                        _server.Server.ReceiveTimeout = (int)serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                    }
                    serverBase.IsStarted = true;

                    while (!IsDispose)
                    {
                        try
                        {
                            //wait and accept new tcp client from newtork
#if (NETSTANDARD1_6)
                            TcpClient client = _server.AcceptTcpClientAsync().GetAwaiter().GetResult();
#else
                            TcpClient client = _server.AcceptTcpClient();
#endif
                            //initialize client to server
                            InitializeClient(client, serverBase);
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    serverBase.OnServerInternalExceptionAction?.Invoke(ex);
                    serverBase.AutoLogger.LogError(ex, "Connect Server");
                }
                finally
                {
                    serverBase.Stop();
                    serverBase.IsStarted = false;
                    Dispose();
                }
            }
        }

        /// <summary>
        /// initialzie and read client
        /// </summary>
        /// <param name="tcpClient">tcp client to read</param>
        /// <param name="serverBase">serverbase for this provider</param>
        public void InitializeClient(TcpClient tcpClient, ServerBase serverBase)
        {
            Task.Run(async () =>
            {
                try
                {
                    //set the timeout of client
                    tcpClient.GetStream().ReadTimeout = 5000;
                    tcpClient.GetStream().WriteTimeout = 5000;
                    //create client stream for read and write to socket
                    PipeNetworkStream stream = new PipeNetworkStream(new NormalStream(await tcpClient.GetTcpStream(serverBase)), (int)serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds);
                    await ExchangeClient(serverBase, stream, tcpClient);
                }
                catch (Exception)
                {
#if (NETSTANDARD)
                    tcpClient.Dispose();
#else
                    tcpClient.Close();
#endif
                }
            });
        }

        /// <summary>
        /// create client information
        /// </summary>
        /// <param name="tcpClient">tcp client connected</param>
        /// <param name="stream">stream of client</param>
        /// <param name="serverBase">serverbase for this provider</param>
        /// <param name="client">the client</param>
        /// <returns></returns>
        public virtual ClientInfo CreateClientInfo(ServerBase serverBase, ClientInfo client, TcpClient tcpClient, PipeNetworkStream stream)
        {
            //set connected time of client
            client.ConnectedDateTime = DateTime.Now;
            client.TcpClient = tcpClient;
            //set client ip address
            client.IPAddressBytes = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.GetAddressBytes();
            //generate clientId
            client.ClientId = Guid.NewGuid();
            //add client to server clients
            serverBase.Clients.TryAdd(client.ClientId, client);
            //set client stream
            client.ClientStream = stream;
            return client;
        }

        /// <summary>
        /// Exchange data from client and server 
        /// </summary>
        /// <param name="serverBase">serverbase for this provider</param>
        /// <param name="streamReader">stream of client</param>
        /// <param name="tcpClient">tcp client connected</param>
        public async virtual Task ExchangeClient(ServerBase serverBase, PipeNetworkStream streamReader, TcpClient tcpClient)
        {
            ClientInfo client = new ClientInfo(serverBase);
            try
            {
                //read first line from server provider
                string firstLineString = await streamReader.ReadLineAsync();

                //check the client protocol is connecting to server
                //if the protocol is signalgo stream
                if (firstLineString.Contains("SignalGo-Stream/4.0"))
                {
                    if (!serverBase.ProviderSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = -1;
                        tcpClient.GetStream().WriteTimeout = -1;
                    }
                    client = CreateClientInfo(serverBase, client, tcpClient, streamReader);
                    client.ProtocolType = ClientProtocolType.SignalGoStream;
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    SignalGoStreamProvider.StartToReadingClientData(client, serverBase);
                }
                //if the protocol is signalgo oneway
                else if (firstLineString.Contains("SignalGo-OneWay/4.0"))
                {
                    if (!serverBase.ProviderSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = -1;
                        tcpClient.GetStream().WriteTimeout = -1;
                    }
                    client = CreateClientInfo(serverBase, client, tcpClient, streamReader);
                    client.ProtocolType = ClientProtocolType.SignalGoOneWay;
                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    OneWayServiceProvider.StartToReadingClientData(client, serverBase);
                }
                //if the protocol is signalgo duplex
                else if (firstLineString.Contains("SignalGo/4.0"))
                {
                    client = CreateClientInfo(serverBase, client, tcpClient, streamReader);
                    client.ProtocolType = ClientProtocolType.SignalGoDuplex;

                    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                    if (serverBase.ProviderSetting.ServerServiceSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.ReceiveTimeout = (int)serverBase.ProviderSetting.ServerServiceSetting.ReceiveDataTimeout.TotalMilliseconds;
                        tcpClient.SendTimeout = (int)serverBase.ProviderSetting.ServerServiceSetting.SendDataTimeout.TotalMilliseconds;
                    }
                    else
                    {
                        tcpClient.GetStream().ReadTimeout = -1;
                        tcpClient.GetStream().WriteTimeout = -1;
                    }
                    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase);
                }
                //if the protocol is http
                else if (firstLineString.Contains("HTTP/"))
                {
                    if (serverBase.ProviderSetting.HttpSetting.IsEnabledToUseTimeout)
                    {
                        tcpClient.GetStream().ReadTimeout = (int)serverBase.ProviderSetting.HttpSetting.ReceiveDataTimeout.TotalMilliseconds;
                        tcpClient.GetStream().WriteTimeout = (int)serverBase.ProviderSetting.HttpSetting.SendDataTimeout.TotalMilliseconds;
                    }
                    await HttpProvider.StartToReadingClientData(tcpClient, serverBase, streamReader, new StringBuilder(firstLineString));
                }
                else
                {
                    serverBase.DisposeClient(client, tcpClient, "AddClient header not support");
                }
            }
            catch (Exception ex)
            {
                serverBase.DisposeClient(client, tcpClient, $"exception: {ex.Message}");
            }
            finally
            {

            }
        }

        /// <summary>
        /// dispose the provider
        /// </summary>
        public void Dispose()
        {
            IsDispose = true;
        }
    }
}
