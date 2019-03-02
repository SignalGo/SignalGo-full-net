using SignalGo.Shared.IO;
using System;
using System.Linq;
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
            return Task.Run(async () =>
            {
                ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
                WebSocketReceiveResult result = await _webSocket.ReceiveAsync(data, new System.Threading.CancellationToken());
                //Array.Copy(buffer, data.Array, result.Count);
                return result.Count;
            });
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (count > WebcoketDatagramBase.MaxLength)
            {
                foreach (byte[] item in WebcoketDatagramBase.GetSegments(buffer.Take(count).ToArray()))
                {
                    ArraySegment<byte> data = new ArraySegment<byte>(item, 0, item.Length);
                    _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                }
            }
            else
            {
                ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
                _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, new System.Threading.CancellationToken()).GetAwaiter().GetResult();
            }
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            if (count > WebcoketDatagramBase.MaxLength)
            {
                foreach (byte[] item in WebcoketDatagramBase.GetSegments(buffer.Take(count).ToArray()))
                {
                    ArraySegment<byte> data = new ArraySegment<byte>(item, 0, item.Length);
                    await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, new System.Threading.CancellationToken());
                }
            }
            else
            {
                ArraySegment<byte> data = new ArraySegment<byte>(buffer, 0, count);
                await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, new System.Threading.CancellationToken());
            }
        }
    }
}
