using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class NormalStream : IStream
    {
        private Stream _stream;
        public NormalStream(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }


        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

#if (!NET35 && !NET40)
        public Task FlushAsync()
        {
            return _stream.FlushAsync();
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            return _stream.ReadAsync(buffer, offset, count);
        }

        public Task WriteAsync(byte[] buffer, int offset, int count)
        {
            return _stream.WriteAsync(buffer, offset, count);
        }
#endif
    }
}
