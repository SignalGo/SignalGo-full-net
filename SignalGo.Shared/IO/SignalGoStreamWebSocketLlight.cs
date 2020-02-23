using SignalGo.Shared.IO.Compressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    /// <summary>
    /// web sokect stream helper without encrypt and decrypt
    /// </summary>
    public class SignalGoStreamWebSocketLlight : SignalGoStreamBase
    {
        static SignalGoStreamWebSocketLlight()
        {
            CurrentWebSocket = new SignalGoStreamWebSocketLlight();
        }

        public static ISignalGoStream CurrentWebSocket { get; set; }


#if (NET35 || NET40)
        public override void WriteToStream(PipeNetworkStream stream, byte[] data)
#else
        public override Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data)
#endif
        {
#if (NET35 || NET40)
            stream.Write(data, 0, data.Length);
#else
            return stream.WriteAsync(data, 0, data.Length);
#endif
        }
#if (NET35 || NET40)
        public override byte[] ReadBlockSize(PipeNetworkStream stream, int count)
#else
        public override async Task<byte[]> ReadBlockSizeAsync(PipeNetworkStream stream, int count)
#endif
        {
            List<byte> bytes = new List<byte>();
            int lengthReaded = 0;

            while (lengthReaded < count)
            {
                int countToRead = count;
                if (lengthReaded + countToRead > count)
                {
                    countToRead = count - lengthReaded;
                }
                //#if (!PORTABLE)
                //                Console.WriteLine("countToRead: " + countToRead);
                //#endif
                byte[] readBytes = new byte[countToRead];
#if (NET35 || NET40)
                int readCount = stream.Read(readBytes, countToRead);
#else
                int readCount = await stream.ReadAsync(readBytes, countToRead);
#endif
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }

#if (NET35 || NET40)
        public override byte[] ReadBlockToEnd(PipeNetworkStream stream, ICompression compression, int maximum)
#else
        public override async Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, ICompression compression, int maximum)
#endif
        {
#if (NET35 || NET40)
            byte[] lenBytes = ReadBlockSize(stream, 4);
            int len = BitConverter.ToInt32(lenBytes, 0);
            var result = ReadBlockSize(stream, len);
#else
            byte[] lenBytes = await ReadBlockSizeAsync(stream, 4);
            int len = BitConverter.ToInt32(lenBytes, 0);
            var result = await ReadBlockSizeAsync(stream, len);
#endif
            return compression.Decompress(ref result);
        }

#if (NET35 || NET40)
        public override byte ReadOneByte(PipeNetworkStream stream)
        {
            return stream.ReadOneByte();
        }
#else
        public override Task<byte> ReadOneByteAsync(PipeNetworkStream stream)
        {
            return stream.ReadOneByteAsync();
        }
#endif
    }
}