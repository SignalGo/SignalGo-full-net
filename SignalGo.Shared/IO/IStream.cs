using System;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public interface IStream : IDisposable
    {
        void Flush();
        int Read(byte[] buffer, int offset, int count);
        void Write(byte[] buffer, int offset, int count);

#if (!NET35 && !NET40)
        Task FlushAsync();
        Task<int> ReadAsync(byte[] buffer, int offset, int count);
        Task WriteAsync(byte[] buffer, int offset, int count);
#endif
    }
}
