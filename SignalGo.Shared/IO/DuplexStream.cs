using System;
using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class DuplexStream : IStream
    {
        private Stream _streamreader;
        private Stream _streamWriter;

        public DuplexStream(Stream streamreader, Stream streamWriter)
        {
            _streamreader = streamreader;
            _streamWriter = streamWriter;

        }
        
        public void Flush()
        {
            _streamWriter.Flush();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _streamreader.Read(buffer, offset, count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _streamWriter.Write(buffer, offset, count);
        }


#if (!NET35 && !NET40)
        public Task FlushAsync()
        {
            return _streamWriter.FlushAsync();
        }

        public Task WriteAsync(byte[] buffer, int offset, int count)
        {
            return _streamWriter.WriteAsync(buffer, offset, count);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            return _streamreader.ReadAsync(buffer, offset, count);
        }
#endif
        public void Dispose()
        {
            _streamreader.Dispose();
            _streamWriter.Dispose();
        }
    }
}
