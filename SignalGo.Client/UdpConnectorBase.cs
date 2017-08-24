using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SignalGo.Shared.Models;
using Newtonsoft.Json;
using SignalGo.Shared;
using System.Collections.Concurrent;
using System.Threading;

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
        Socket socket = null;
        IPEndPoint iPEndPoint = null;
        /// <summary>
        /// connect to socket
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void ConnectToUDP(string ipAddress, int port)
        {
            isStart = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            iPEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            StartReadingData();
            AsyncActions.Run(() =>
            {
                StartEngineWriter();
            });
            SendUdpData(new byte[] { 0 });
        }

        public int BufferSize { get; set; } = 50000;

        //start to reading data from server
        void StartReadingData()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    socket.Connect(iPEndPoint);
                    while (!IsDisposed)
                    {
                        byte[] bytes = new byte[BufferSize];
                        var readCount = socket.Receive(bytes);
                        OnReceivedData?.Invoke(bytes.ToList().GetRange(0, readCount).ToArray());
                    }
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

        BlockingCollection<byte[]> BytesToSend { get; set; } = new BlockingCollection<byte[]>();
        bool isStart = false;
        void StartEngineWriter()
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
                var sendCount = socket.SendTo(arrayToSend, arrayToSend.Length, SocketFlags.None, iPEndPoint);

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

        internal void DisposedClient(TcpClient client)
        {
            base.Dispose();
            if (socket != null)
#if (NETSTANDARD1_6 || NETCOREAPP1_1)
                socket.Dispose();
#else
                socket.Close();
#endif
            BytesToSend.Add(new byte[0]);
        }
    }
}
