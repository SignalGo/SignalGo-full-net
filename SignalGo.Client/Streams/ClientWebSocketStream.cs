#if (NETSTANDARD2_0 || NET45)
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Client.Streams
{
    public class ClientWebSocketStream : Stream
    {
        private ClientWebSocket _clientWebSocket;

        public ClientWebSocketStream(ClientWebSocket clientWebSocket)
        {
            _clientWebSocket = clientWebSocket;
            _clientWebSocket.Options.KeepAliveInterval = TimeSpan.MaxValue;
            _clientWebSocket.Options.SetBuffer(ushort.MaxValue, ushort.MaxValue);

        }

        public override bool CanRead
        {
            get
            {
                return _clientWebSocket.State == WebSocketState.Open;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _clientWebSocket.State == WebSocketState.Open;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_clientWebSocket)
            {
                WebSocketReceiveResult result = _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                return result.Count;
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            WebSocketReceiveResult result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            return result.Count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_clientWebSocket)
            {
                 _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Take(1).ToArray()), WebSocketMessageType.Binary, true, new CancellationToken()).GetAwaiter().GetResult();
                 _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Skip(1).Take(1).ToArray()), WebSocketMessageType.Binary, true, new CancellationToken()).GetAwaiter().GetResult();
                if (buffer.Length > 2)
                {
                     //_clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Skip(2).Take(4).ToArray()), WebSocketMessageType.Binary, true, new CancellationToken()).GetAwaiter().GetResult();
                     _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Skip(2).ToArray()), WebSocketMessageType.Binary, true, new CancellationToken()).GetAwaiter().GetResult();
                }
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Take(1).ToArray()), WebSocketMessageType.Binary, true, cancellationToken);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Skip(1).Take(1).ToArray()), WebSocketMessageType.Binary, true, cancellationToken);
            if (buffer.Length > 2)
            {
                //await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Skip(2).Take(4).ToArray()), WebSocketMessageType.Binary, true, cancellationToken);
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer.Skip(2).ToArray()), WebSocketMessageType.Binary, true, cancellationToken);
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }
        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            Write(new byte[] { value }, 0, 1);
        }
    }
}
#endif
