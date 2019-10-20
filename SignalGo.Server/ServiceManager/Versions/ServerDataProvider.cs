// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager.Providers;
using SignalGo.Shared.Helpers;
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
    public class ServerDataProvider : ServerDataProviderBase
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
                    //create instance of tcp listener to lister for clients
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
                    //tcpClient.GetStream().ReadTimeout = 5000;
                    //tcpClient.GetStream().WriteTimeout = 5000;
                    //create client stream for read and write to socket
                    PipeLineStream stream = new PipeLineStream(tcpClient.GetTcpStream(serverBase));
                    await ExchangeClientFunc(serverBase, stream, tcpClient);
                }
                catch (Exception)
                {
#if (NET45)
                    tcpClient.Close();
#else
                    tcpClient.Dispose();
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
        /// <returns>new client information</returns>
        public virtual ClientInfo CreateClient(ServerBase serverBase, ClientInfo client, TcpClient tcpClient, PipeLineStream stream)
        {
            //set connected time of client
            client.ConnectedDateTime = DateTime.Now;
            client.TcpClient = tcpClient;
            do
            {
                //generate clientId
                client.ClientId = Guid.NewGuid();
            }
            //add client to server clients
            while (!serverBase.Clients.TryAdd(client.ClientId, client));
            //set client stream
            client.ClientStream = stream;
            return client;
        }

        /// <summary>
        /// call on client connected action
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="client"></param>
        /// <param name="tcpClient"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public ClientInfo CreateClientInfoOnConnectionStatusChanged(ServerBase serverBase, ClientInfo client, TcpClient tcpClient, PipeLineStream stream)
        {
            var result = CreateClient(serverBase, client, tcpClient, stream);
            serverBase.OnClientConnectedAction?.Invoke(client);
            return result;
        }

        /// <summary>
        /// Exchange data from client and server 
        /// </summary>
        /// <param name="serverBase">serverbase for this provider</param>
        /// <param name="streamReader">stream of client</param>
        /// <param name="tcpClient">tcp client connected</param>
        internal async Task ExchangeClient(ServerBase serverBase, PipeLineStream streamReader, TcpClient tcpClient)
        {
            ClientInfo client = new ClientInfo(serverBase, tcpClient);
            try
            {
                //read all of the lines in connection
                //this will show us headers and protocol type
                await streamReader.ReadAllLinesAsync();

                //check the client protocol is connecting to server

                //if the protocol is http
                if (streamReader.ProtocolType == Shared.Enums.ProtocolType.Http)
                {
                    await HttpProvider.StartToReadingClientData(tcpClient, serverBase, streamReader, client);
                }
                //if the protocol is signalgo duplex
                //else if (firstLineString.Contains("SignalGo/6.0"))
                //{
                //    client = CreateClientFunc(serverBase, client, tcpClient, streamReader);
                //    client.ProtocolType = ClientProtocolType.SignalGoDuplex;
                //    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                //    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase);
                //}
                ////if the protocol is signalgo duplex
                //else if (firstLineString.Contains(TextHelper.SignalGoVersion_4_FirstLine))
                //{
                //    client = CreateClientFunc(serverBase, client, tcpClient, streamReader);
                //    client.ProtocolType = ClientProtocolType.SignalGoDuplex;
                //    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                //    await SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase);
                //}
                ////if the protocol is signalgo stream
                //if (firstLineString.Contains("SignalGo-Stream/4.0"))
                //{
                //    client = CreateClientFunc(serverBase, client, tcpClient, streamReader);
                //    client.ProtocolType = ClientProtocolType.SignalGoStream;
                //    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                //    SignalGoStreamProvider.StartToReadingClientData(client, serverBase);
                //}
                ////if the protocol is signalgo oneway
                //else if (firstLineString.Contains("SignalGo-OneWay/4.0"))
                //{
                //    client = CreateClientFunc(serverBase, client, tcpClient, streamReader);
                //    client.ProtocolType = ClientProtocolType.SignalGoOneWay;
                //    client.StreamHelper = SignalGoStreamBase.CurrentBase;
                //    OneWayServiceProvider.StartToReadingClientData(client, serverBase);
                //}
                //else
                //{
                //    serverBase.DisposeClient(client, tcpClient, "AddClient header not support");
                //}
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
        /// set client streams timeouts
        /// </summary>
        /// <param name="serverBase"></param>
        /// <param name="streamReader"></param>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        internal Task ExchangeClientTimeOut(ServerBase serverBase, PipeLineStream streamReader, TcpClient tcpClient)
        {
            var result = ExchangeClient(serverBase, streamReader, tcpClient);
            //set client timeouts
            if (serverBase.ProviderSetting.IsEnabledToUseTimeout)
            {
                tcpClient.ReceiveTimeout = (int)serverBase.ProviderSetting.ReceiveDataTimeout.TotalMilliseconds;
                tcpClient.SendTimeout = (int)serverBase.ProviderSetting.SendDataTimeout.TotalMilliseconds;
            }
            else
            {
                tcpClient.GetStream().ReadTimeout = -1;
                tcpClient.GetStream().WriteTimeout = -1;
            }
            return result;
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
