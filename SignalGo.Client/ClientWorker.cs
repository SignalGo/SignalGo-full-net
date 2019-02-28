#if (NETSTANDARD2_0 || NET45)
using SignalGo.Client.Streams;
using System.Net.WebSockets;
#endif
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using SignalGo.Shared.Helpers;
using System.Text;

namespace SignalGo.Client
{
    /// <summary>
    /// client connection and data exchanger manager interface
    /// </summary>
    public interface IClientWorker : IDisposable
    {
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        void Connect(string address, int port);
#if (!NET35 && !NET40)
        /// <summary>
        /// connect to server async
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        Task ConnectAsync(string address, int port);
#endif
        /// <summary>
        /// get stream of socket
        /// </summary>
        /// <returns></returns>
        Stream GetStream();
    }

    /// <summary>
    /// tcp pure socket manager
    /// </summary>
    public class TcpClientWorker : IClientWorker
    {
        private TcpClient _tcpClient;
        public TcpClientWorker(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void Connect(string address, int port)
        {
#if (NETSTANDARD1_6)
            _tcpClient.ConnectAsync(address, port).GetAwaiter().GetResult();
#else
            _tcpClient.Connect(address, port);
#endif
        }
#if (!NET35 && !NET40)
        /// <summary>
        /// connect to server asyc
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Task ConnectAsync(string address, int port)
        {
            return _tcpClient.ConnectAsync(address, port);
        }
#endif

        /// <summary>
        /// dispose client
        /// </summary>
        public void Dispose()
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            _tcpClient.Dispose();
#else
            _tcpClient.Close();
#endif
        }

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }
    }
#if (NETSTANDARD2_0 || NET45)
    /// <summary>
    /// web socket manager
    /// </summary>
    public class WebSocketClientWorker : IClientWorker
    {
        private TcpClient _tcpClient;
        //private ClientWebSocket _clientWebSocket;
        //readonly ClientWebSocketStream _stream = null;
        public WebSocketClientWorker(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }
        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void Connect(string address, int port)
        {
            if (port == 443)
                address = "wss://" + address;
            else
                address = "ws://" + address;
            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri))
                throw new Exception($"cannot parse uri {address}");
            //string headData = $"GET {uri.AbsolutePath} HTTP/1.1{newLine}Host: {uri.Host + port}{newLine}Connection: keep-alive{newLine}Sec-WebSocket-Key: x3JJHMbDL1EzLkh9GBhXDw=={newLine}Sec-WebSocket-Protocol: chat, superchat{newLine}Sec-WebSocket-Version: 13{newLine + newLine}";
            byte[] firstBytes = GetFirstLineBytes(uri.Host, port);
            _tcpClient.Connect(uri.Host, port);
            //_clientWebSocket.Options.SetRequestHeader("SignalgoDuplexWebSocket", "true");
            _tcpClient.GetStream().Write(firstBytes, 0, firstBytes.Length);
        }

        private byte[] GetFirstLineBytes(string host, int port)
        {
            string newLine = TextHelper.NewLine;
            string headData = $@"GET / HTTP/1.1
Host: {host}:{port}
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate
Sec-WebSocket-Version: 13
Origin: null
SignalgoDuplexWebSocket: true
Sec-WebSocket-Extensions: permessage-deflate
Sec-WebSocket-Key: Kf1/4OQ83wCdLhsU8LkwqQ==
DNT: 1
Connection: keep-alive, Upgrade
Pragma: no-cache
Cache-Control: no-cache
Upgrade: websocket{newLine + newLine}";
            return Encoding.UTF8.GetBytes(headData);
        }

        /// <summary>
        /// connect to server async
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string address, int port)
        {
            if (port == 443)
                address = "wss://" + address;
            else
                address = "ws://" + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri))
                throw new Exception($"cannot parse uri {address}");
            byte[] firstBytes = GetFirstLineBytes(uri.Host, port);
            await _tcpClient.ConnectAsync(uri.Host, port);
            await _tcpClient.GetStream().WriteAsync(firstBytes, 0, firstBytes.Length);
        }

        /// <summary>
        /// dispose client
        /// </summary>
        public void Dispose()
        {
#if (NETSTANDARD1_6 || NETCOREAPP1_1 || PORTABLE)
            _tcpClient.Dispose();
#else
            _tcpClient.Close();
#endif
        }

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }
    }
#endif
}
