using SignalGo.Shared.IO;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SignalGo.Server.Owin
{
    public class WebsocketStream : IStream
    {
        private WebSocket _webSocket;
        public WebsocketStream(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public int ReceiveTimeout { get; set; } = -1;

        public int SendTimeout { get; set; } = -1;

        public void Dispose()
        {

        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
            WebSocketReceiveResult result = _webSocket.ReceiveAsync(data, new System.Threading.CancellationToken()).GetAwaiter().GetResult();
            Array.Copy(buffer, data.Array, result.Count);
            return result.Count;
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            return Task.Run(() =>
            {
                ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
                WebSocketReceiveResult result = _webSocket.ReceiveAsync(data, new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                //Array.Copy(buffer, data.Array, result.Count);
                return result.Count;
            });
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
            _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, new System.Threading.CancellationToken()).GetAwaiter().GetResult();
        }

        public Task WriteAsync(byte[] buffer, int offset, int count)
        {
            ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
            return _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, new System.Threading.CancellationToken());
        }
    }
}
