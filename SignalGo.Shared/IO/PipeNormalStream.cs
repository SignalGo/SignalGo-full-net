using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class PipeNormalStream : Stream
    {
        private PipeNetworkStream _pipeNetworkStream;
        public PipeNormalStream(PipeNetworkStream pipeNetworkStream)
        {
            _pipeNetworkStream = pipeNetworkStream;
        }

        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

#if (NET35 || NET40)
        public override int Read(byte[] buffer, int offset, int count)
        {
            int readCount = _pipeNetworkStream.Read(buffer, count);
            return readCount;
        }
#else
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int readCount = await _pipeNetworkStream.ReadAsync(buffer, count);
            return readCount;
        }
#endif

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
#if (!NET35 && !NET40)
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
#endif

#if (NET35 || NET40)
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length != count)
                _pipeNetworkStream.Write(buffer.Take(count).ToArray(), offset, count);
            else
                _pipeNetworkStream.Write(buffer, offset, count);
        }
#else
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer.Length != count)
                return _pipeNetworkStream.WriteAsync(buffer.Take(count).ToArray(), offset, count);
            else
                return _pipeNetworkStream.WriteAsync(buffer, offset, count);
        }
#endif
    }
}
