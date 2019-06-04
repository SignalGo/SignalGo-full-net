using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public abstract class WebcoketDatagramBase
    {
        public static int MaxLength { get; set; } = 10000;
        public static WebcoketDatagramBase Current { get; set; }
        public abstract int GetLength(byte[] bytes);
        public abstract Tuple<int, byte[]> GetBlockLength(Stream stream, Func<int, byte[]> readBlockSize);
#if (!NET35 && !NET40)
        public abstract Task<Tuple<int, byte[]>> GetBlockLengthAsync(Stream stream, Func<int, Task<byte[]>> readBlockSizeAsync);
#endif
        public abstract byte[] Encode(byte[] bytes);
        public abstract byte[] Dencode(byte[] bytes);

        public static byte[][] GetSegments(byte[] bytes)
        {
            List<byte[]> result = new List<byte[]>();
            int skip = 0;
            while (skip < bytes.Length)
            {
                result.Add(bytes.Skip(skip).Take(MaxLength).ToArray());
                skip += MaxLength;
            }
            return result.ToArray();
        }
    }
}
