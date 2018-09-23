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

        public int ReceiveTimeout
        {
            get
            {
                return _stream.ReadTimeout;
            }
            set
            {
                _stream.ReadTimeout = value;
            }
        }

        public int SendTimeout
        {
            get
            {
                return _stream.WriteTimeout;
            }
            set
            {
                _stream.WriteTimeout = value;
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

#if (NET35 || NET40)
        public void Flush()
        {
            _stream.Flush();
        }
#else
   public Task FlushAsync()
        {
            return _stream.FlushAsync();
        }
#endif
#if (NET35 || NET40)
        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }
#else
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            if (_stream.ReadTimeout > 0)
            {
                int ReciveCount = 0;
                Task receiveTask = Task.Run(async () => { ReciveCount = await _stream.ReadAsync(buffer, offset, count); });
                bool isReceived = await Task.WhenAny(receiveTask, Task.Delay(_stream.ReadTimeout)) == receiveTask;
                if (!isReceived)
                    return -1;
                return ReciveCount;
            }
            else
            {
                return await _stream.ReadAsync(buffer, offset, count);
            }
        }
#endif

#if (NET35 || NET40)
        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
#else
        public Task WriteAsync(byte[] buffer, int offset, int count)
        {
            return _stream.WriteAsync(buffer, offset, count);
        }
#endif
    }
}
