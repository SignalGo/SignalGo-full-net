using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class WebSocketStream : IStream
    {
        private readonly Stream _stream;
        public WebSocketStream(Stream stream)
        {
            _stream = stream;
        }

        //public static ISignalGoStream CurrentWebSocket { get; set; }


        //#if (NET35 || NET40)
        //        public override void WriteToStream(PipeNetworkStream stream, byte[] data)
        //#else
        //        public override Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data)
        //#endif
        //        {
        //            byte[] encode = WebcoketDatagramBase.Current.Encode(data);
        //#if (NET35 || NET40)
        //            stream.Write(encode, 0, encode.Length);
        //#else
        //            return stream.WriteAsync(encode, 0, encode.Length);
        //#endif
        //        }

        //#if (!NET35 && !NET40)
        //        public override void WriteToStream(PipeNetworkStream stream, byte[] data)
        //        {
        //            WriteToStreamAsync(stream, data).GetAwaiter().GetResult();
        //        }

        //        public override byte[] ReadBlockSize(PipeNetworkStream stream, int count)
        //        {
        //            return ReadBlockSizeAsync(stream, count).GetAwaiter().GetResult();
        //        }
        //        public override byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum)
        //        {
        //            return ReadBlockToEndAsync(stream, compress, maximum).GetAwaiter().GetResult();
        //        }
        //        public override byte ReadOneByte(PipeNetworkStream stream)
        //        {
        //            return ReadOneByteAsync(stream).GetAwaiter().GetResult();
        //        }
        //#endif


        //#if (NET35 || NET40)
        //        public override byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum)
        //#else
        //        public override async Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, CompressMode compress, int maximum)
        //#endif
        //        {
        //#if (NET35 || NET40)
        //            Tuple<int, byte[]> data = WebcoketDatagramBase.Current.GetBlockLength(stream, ReadBlockSize);
        //#else
        //            Tuple<int, byte[]> data = await WebcoketDatagramBase.Current.GetBlockLengthAsync(stream, ReadBlockSizeAsync);
        //#endif
        //            byte[] buffer = data.Item2;
        //#if (NET35 || NET40)
        //            byte[] newBytes = ReadBlockSize(stream, data.Item1);
        //#else
        //            byte[] newBytes = await ReadBlockSizeAsync(stream, data.Item1);
        //#endif
        //            List<byte> b = new List<byte>();
        //            b.AddRange(buffer);
        //            b.AddRange(newBytes);
        //            byte[] decode = WebcoketDatagramBase.Current.Dencode(b.ToArray());
        //            if (decode == null || decode.Length == 0)
        //            {
        //                throw new Exception("websocket closed by client");
        //            }
        //            return decode;
        //        }

        //#if (NET35 || NET40)
        //        public override byte ReadOneByte(PipeNetworkStream stream)
        //        {
        //            byte[] dataBytes = ReadBlockToEnd(stream, CompressMode.None, 1);
        //            return dataBytes[0];
        //        }
        //#else
        //        public override async Task<byte> ReadOneByteAsync(PipeNetworkStream stream)
        //        {
        //            byte[] dataBytes = await ReadBlockToEndAsync(stream, CompressMode.None, 1);
        //            return dataBytes[0];
        //        }

        //        public override Task<string> ReadLineAsync(PipeNetworkStream stream, string exitCode)
        //        {
        //            return base.ReadLineAsync(stream, exitCode);
        //        }
        //#endif
#if (!NET35 && !NET40)
        public byte[] ReadBlockSize(int count)
        {
            Debug.WriteLine("DeadLock Warning ReadBlockSize!");
            return ReadBlockSizeAsync(count).GetAwaiter().GetResult();
        }
#endif

#if (NET35 || NET40)
        public byte[] ReadBlockSize(int count)
#else
        public async Task<byte[]> ReadBlockSizeAsync(int count)
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
                int readCount = _stream.Read(readBytes, 0, countToRead);
#else
                int readCount = await _stream.ReadAsync(readBytes, 0, countToRead);
#endif
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }

        public int ReceiveTimeout { get; set; }

        public int SendTimeout { get; set; }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public void Flush()
        {
            _stream.Flush();
        }

# if (!NET35 && !NET40)
        public Task FlushAsync()
        {
            return _stream.FlushAsync();
        }
#endif

        public int Read(byte[] buffer, int offset, int count)
        {
            Tuple<int, byte[]> data = WebcoketDatagramBase.Current.GetBlockLength(_stream, ReadBlockSize);
            byte[] newBytes = ReadBlockSize(data.Item1);
            List<byte> b = new List<byte>();
            b.AddRange(data.Item2);
            b.AddRange(newBytes);
            byte[] decode = WebcoketDatagramBase.Current.Dencode(b.ToArray());
            if (decode == null || decode.Length == 0)
            {
                throw new Exception("websocket closed by client");
            }
            else if (count < decode.Length)
                throw new Exception($"your count request is {count} but i read {decode.Length} from stream in websocket!");
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = decode[i];
            }
            return decode.Length;
        }

# if (!NET35 && !NET40)
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            Tuple<int, byte[]> data = await WebcoketDatagramBase.Current.GetBlockLengthAsync(_stream, ReadBlockSizeAsync);
            byte[] newBytes = await ReadBlockSizeAsync(data.Item1);
            List<byte> b = new List<byte>();
            b.AddRange(data.Item2);
            b.AddRange(newBytes);
            byte[] decode = WebcoketDatagramBase.Current.Dencode(b.ToArray());
            if (decode == null || decode.Length == 0)
            {
                throw new Exception("websocket closed by client");
            }
            else if (count < decode.Length)
                throw new Exception($"your count request is {count} but i read {decode.Length} from stream in websocket!");
            for (int i = 0; i < decode.Length; i++)
            {
                buffer[i] = decode[i];
            }
            return decode.Length;
        }
#endif

        public void Write(byte[] buffer, int offset, int count)
        {
            if (count > WebcoketDatagramBase.MaxLength)
            {
                foreach (byte[] item in WebcoketDatagramBase.GetSegments(buffer.Take(count).ToArray()))
                {
                    byte[] encode = WebcoketDatagramBase.Current.Encode(item);
                    _stream.Write(encode, 0, encode.Length);
                }
            }
            else
            {
                byte[] encode = WebcoketDatagramBase.Current.Encode(buffer.Take(count).ToArray());
                _stream.Write(encode, 0, encode.Length);
            }
        }

# if (!NET35 && !NET40)
        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            if (count > WebcoketDatagramBase.MaxLength)
            {
                foreach (byte[] item in WebcoketDatagramBase.GetSegments(buffer.Take(count).ToArray()))
                {
                    byte[] encode = WebcoketDatagramBase.Current.Encode(item);
                    await _stream.WriteAsync(encode, 0, encode.Length);
                }
            }
            else
            {
                byte[] encode = WebcoketDatagramBase.Current.Encode(buffer.Take(count).ToArray());
                //byte[] decode = WebcoketDatagramBase.Current.Dencode(encode);
                await _stream.WriteAsync(encode, 0, encode.Length);
            }
        }
#endif
    }
}
