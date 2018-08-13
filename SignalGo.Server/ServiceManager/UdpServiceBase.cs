using SignalGo.Server.Models;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager
{
    public abstract class UdpServiceBase : ServerStreamBase
    {
//        //public bool IsManagedUdpClients { get; set; } = true;
//        UdpClient newsock = null;
//        public void ConnectToUDP(int port)
//        {
//            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
//            newsock = new UdpClient(ipep);
//            AsyncActions.Run(() =>
//            {
//                StartToReadingData();
//            });
//        }
//#if (NETSTANDARD1_6 || NETCOREAPP1_1)
//        async void StartToReadingData()
//#else
//        void StartToReadingData()
//#endif
//        {
//            while (!IsDisposed)
//            {
//                try
//                {
//                    Console.WriteLine("start to read data...");
//                    IPEndPoint sender = null;
//                    byte[] byffer = null;
//#if (NETSTANDARD1_6 || NETCOREAPP1_1)
//                    var data = await newsock.ReceiveAsync();
//                    sender = data.RemoteEndPoint;
//                    byffer = data.Buffer;
//#else
//                    byffer = newsock.Receive(ref sender);
//#endif
//                    Console.WriteLine("receive data : " + byffer.Length);
//                    if (!ClientsByIp.ContainsKey(sender.Address.ToString()))
//                    {
//                        Console.WriteLine("not found client " + sender.Address.ToString() + " count: " + ClientsByIp.Count);
//                        continue;
//                    }

//                    var clients = ClientsByIp[sender.Address.ToString()];
//                    foreach (var item in clients)
//                    {
//                        if (!UDPClients.ContainsKey(item))
//                        {
//                            UDPClients.TryAdd(item, new BlockingCollection<byte[]>());
//                            //start engine
//                            CreateNewEngine(item);
//                        }
//                        item.UdpIp = sender;
//                    }
//                    if (byffer.Length == 1)
//                    {
//                        Console.WriteLine("client connected: " + sender.Address.ToString());
//                        continue;
//                    }
//                    foreach (var item in UDPClients)
//                    {
//                        if (item.Key.IPAddress == sender.Address.ToString())
//                            continue;
//                        bool isAdd = UDPClients[item.Key].TryAdd(byffer);
//                        if (!isAdd)
//                            Console.WriteLine("cannot tryAdd to client");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex);
//                }
//            }
//        }

//        ConcurrentDictionary<ClientInfo, BlockingCollection<byte[]>> UDPClients = new ConcurrentDictionary<ClientInfo, BlockingCollection<byte[]>>();
//        void CreateNewEngine(ClientInfo client)
//        {
//            var blocks = UDPClients[client];
//            AsyncActions.Run(() =>
//            {
//                StartEngineToSendData(client, blocks);
//            }, (ex) =>
//            {
//                UDPClients.Remove(client);
//            });
//        }

//#if (NETSTANDARD1_6 || NETCOREAPP1_1)
//        async void StartEngineToSendData(ClientInfo client, BlockingCollection<byte[]> blocks)
//#else
//        void StartEngineToSendData(ClientInfo client,BlockingCollection<byte[]> blocks)
//#endif
//        {
//            try
//            {
//                while (!IsDisposed)
//                {
//                    byte[] arrayToSend = null;
//                    arrayToSend = blocks.Take();
//                    if (IsDisposed)
//                        break;
//#if (NETSTANDARD1_6 || NETCOREAPP1_1)
//                    var count = await newsock.SendAsync(arrayToSend, arrayToSend.Length, client.UdpIp);
//#else
//                    var count =  newsock.Send(arrayToSend, arrayToSend.Length, client.UdpIp);
//#endif
//                    if (count != arrayToSend.Length)
//                        Console.WriteLine("data lose to send...");
//                }
//            }
//            catch (Exception ex)
//            {
//                UDPClients.Remove(client);
//                Console.WriteLine(ex);
//                ServerExtension.SendCustomCallbackToClient(client, new Shared.Models.MethodCallInfo() { Type = Shared.Models.MethodType.SignalGo, MethodName = "/MustReconnectUdpServer" });
//            }
//        }
    }
}
