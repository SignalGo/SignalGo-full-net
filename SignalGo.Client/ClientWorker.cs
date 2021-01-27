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
using System.Net.Security;

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
            Debug.WriteLine("DeadLock Warning TcpClientWorker Connect!");
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
    /// <summary>
    /// web socket manager
    /// </summary>
    public class WebSocketClientWorker : IClientWorker
    {
        private TcpClient _tcpClient;
        private Stream _stream;
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
            string host = address;
            if (port == 443)
                address = "wss://" + address;
            else
                address = "ws://" + address;
            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri))
                throw new Exception($"cannot parse uri {address}");
            //string headData = $"GET {uri.AbsolutePath} HTTP/1.1{newLine}Host: {uri.Host + port}{newLine}Connection: keep-alive{newLine}Sec-WebSocket-Key: x3JJHMbDL1EzLkh9GBhXDw=={newLine}Sec-WebSocket-Protocol: chat, superchat{newLine}Sec-WebSocket-Version: 13{newLine + newLine}";
            byte[] firstBytes = GetFirstLineBytes(uri.Host, port);
#if (NETSTANDARD1_6)
            Debug.WriteLine("DeadLock Warning WebSocketClientWorker Connect!");
            _tcpClient.ConnectAsync(uri.Host, port).GetAwaiter().GetResult();
#else
            _tcpClient.Connect(uri.Host, port);
#endif
            if (port == 443)
            {
#if (NETSTANDARD1_6)
                throw new NotSupportedException("not support ssl connection in NETSTANDARD 1.6");
#else
                SslStream sslStream = new SslStream(_tcpClient.GetStream());
                sslStream.AuthenticateAsClient(host);
                _stream = sslStream;
#endif
            }
            else
            {
                _stream = _tcpClient.GetStream();
            }
            _stream.Write(firstBytes, 0, firstBytes.Length);
        }

        private byte[] GetFirstLineBytes(string host, int port)
        {
            string newLine = TextHelper.NewLine;
            string headData = $@"GET / HTTP/1.1
Host: {host}:{port}
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
            return Encoding.ASCII.GetBytes(headData);
        }

#if (!NET35 && !NET40)
        /// <summary>
        /// connect to server async
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string address, int port)
        {
            string host = address;
            if (port == 443)
                address = "wss://" + address;
            else
                address = "ws://" + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri))
                throw new Exception($"cannot parse uri {address}");
            byte[] firstBytes = GetFirstLineBytes(uri.Host, port);
            await _tcpClient.ConnectAsync(uri.Host, port);
            if (port == 443)
            {
#if (NETSTANDARD1_6)
                throw new NotSupportedException("not support ssl connection in NETSTANDARD 1.6");
#else
                SslStream sslStream = new SslStream(_tcpClient.GetStream());
                await sslStream.AuthenticateAsClientAsync(host);
                _stream = sslStream;
#endif
            }
            else
            {
                _stream = _tcpClient.GetStream();
            }
            await _stream.WriteAsync(firstBytes, 0, firstBytes.Length);
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
            return _stream;
        }
    }
}
