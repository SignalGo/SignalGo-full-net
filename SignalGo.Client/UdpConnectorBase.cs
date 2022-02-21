using SignalGo.Shared.Log;
using System;
using System.Linq;
using System.Net;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Threading.Tasks;
using SignalGo.Shared.Models;
using Newtonsoft.Json;
using SignalGo.Shared;
using System.Collections.Concurrent;

namespace SignalGo.Client
{
    /// <summary>
    /// udp data send and receive for sound and video
    /// </summary>
    public class UdpConnectorBase : ConnectorStreamBase
    {
        /// <summary>
        /// received data action
        /// </summary>
        public Action<byte[]> OnReceivedData { get; set; }
#if (!PORTABLE)
        private Socket socket = null;
        private IPEndPoint iPEndPoint = null;
#else
        Sockets.Plugin.UdpSocketClient socket = null;
        string _ipAddress = null;
        int _port = 0;
#endif
        /// <summary>
        /// connect to socket
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void ConnectToUDP(string ipAddress, int port)
        {
            isStart = false;
#if (!PORTABLE)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
            iPEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
#else
            socket = new Sockets.Plugin.UdpSocketClient();
            _ipAddress = ipAddress;
            _port = port;
#endif
            StartReadingData();
            AsyncActions.Run(() =>
            {
                StartEngineWriter();
            });
            SendUdpData(new byte[] { 0 });
        }

        public int BufferSize { get; set; } = 50000;

        //start to reading data from server
#if (PORTABLE)
        void StartReadingData()
#else
        private void StartReadingData()
#endif
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
#if (PORTABLE)
                    socket.ConnectAsync(_ipAddress, _port).Wait();
#else
                    socket.Connect(iPEndPoint);
#endif
#if (PORTABLE)
                    ManualResetEvent ev = new ManualResetEvent(true);
                    socket.MessageReceived += (s, e) =>
                    {
                        OnReceivedData?.Invoke(e.ByteData);
                    };

                    ev.WaitOne();
#else
                    while (!IsDisposed)
                    {
                        byte[] bytes = new byte[BufferSize];
                        int readCount = socket.Receive(bytes);
                        OnReceivedData?.Invoke(bytes.ToList().GetRange(0, readCount).ToArray());
                    }
#endif
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "UdpConnectorBase StartReadingData");
                }
            });
        }

        /// <summary>
        /// send upd data to server
        /// </summary>
        /// <param name="bytes"></param>
        public void SendUdpData(byte[] bytes)
        {
            if (!BytesToSend.TryAdd(bytes))
            {
                AutoLogger.LogText("cannot add udp block");
            }
        }

        private BlockingCollection<byte[]> BytesToSend { get; set; } = new BlockingCollection<byte[]>();

        private bool isStart = false;

        private void StartEngineWriter()
        {
            if (isStart)
                return;
            isStart = true;
            while (!IsDisposed)
            {
                byte[] arrayToSend = null;
                arrayToSend = BytesToSend.Take();
                if (IsDisposed)
                    break;
#if (PORTABLE)
                socket.SendToAsync(arrayToSend, arrayToSend.Length, _ipAddress, _port).Wait();

#else
                int sendCount = socket.SendTo(arrayToSend, arrayToSend.Length, SocketFlags.None, iPEndPoint);
#endif

            }
            isStart = false;
        }

        internal override void ReconnectToUdp(MethodCallInfo callInfo)
        {
            AutoLogger.LogText("ReconnectToUdp");
            MethodCallbackInfo callback = new MethodCallbackInfo();
            callback.Guid = callInfo.Guid;
            try
            {
                SendUdpData(new byte[] { 0 });
            }
            catch (Exception ex)
            {
                AutoLogger.LogError(ex, "UdpConnectorBase ReconnectToUdp");
                callback.IsException = true;
                callback.Data = JsonConvert.SerializeObject(ex.ToString());
            }
            SendCallbackData(callback);
        }
#if (PORTABLE)
        internal void DisposedClient(Sockets.Plugin.UdpSocketClient client)
#else
        internal void DisposedClient(TcpClient client)
#endif
        {
            base.Dispose();
            if (socket != null)
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
                socket.Dispose();
#else
                socket.Close();
#endif
            BytesToSend.Add(new byte[0]);
        }
    }
}
