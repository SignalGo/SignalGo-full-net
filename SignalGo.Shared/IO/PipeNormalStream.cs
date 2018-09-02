using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class PipeNormalStream : Stream
    {
        PipeNetworkStream _pipeNetworkStream;
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            buffer = _pipeNetworkStream.Read(count, out int readCount);
            return readCount;
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
            if (buffer.Length != count)
                _pipeNetworkStream.Write(buffer.Take(count).ToArray());
            else
                _pipeNetworkStream.Write(buffer);
        }
    }
}
