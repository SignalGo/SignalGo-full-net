using System;
using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public abstract class WebcoketDatagramBase
    {
        public static WebcoketDatagramBase Current { get; set; }
        public abstract int GetLength(byte[] bytes);
        public abstract Tuple<int, byte[]> GetBlockLength(Stream stream, Func<int, byte[]> readBlockSize);
#if (!NET35 && !NET40)
        public abstract Task<Tuple<int, byte[]>> GetBlockLengthAsync(Stream stream, Func<int, Task<byte[]>> readBlockSizeAsync);
#endif
        public abstract byte[] Encode(byte[] bytes);
        public abstract byte[] Dencode(byte[] bytes);
    }
}
