using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class SignalGoStreamBase : ISignalGoStream
    {
        static SignalGoStreamBase()
        {
            CurrentBase = new SignalGoStreamBase();
        }


        public static ISignalGoStream CurrentBase { get; set; }


        public virtual byte[] EncodeMessageToSend(byte[] bytesRaw)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// read data from stream
        /// </summary>
        /// <param name="stream">your stream to read a block</param>
        /// <param name="compress">compress mode</param>
        /// <returns>return a block byte array</returns>

#if (NET35 || NET40)
        public virtual byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum)
#else
        public virtual async Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, CompressMode compress, int maximum)
#endif
        {
            //first 4 bytes are size of block
#if (NET35 || NET40)
            byte[] dataLenByte = ReadBlockSize(stream, 4);
#else
            byte[] dataLenByte = await ReadBlockSizeAsync(stream, 4);
#endif
            //convert bytes to int
            int dataLength = BitConverter.ToInt32(dataLenByte, 0);
            if (dataLength > maximum)
                throw new Exception("dataLength is upper than maximum :" + dataLength);
            //read a block
#if (NET35 || NET40)
            byte[] dataBytes = ReadBlockSize(stream, dataLength);
#else
            byte[] dataBytes = await ReadBlockSizeAsync(stream, dataLength);
#endif
            return dataBytes;
        }

#if (NET35 || NET40)
        public void WriteBlockToStream(PipeNetworkStream stream, byte[] data)
#else
        public Task WriteBlockToStreamAsync(PipeNetworkStream stream, byte[] data)
#endif
        {
            byte[] size = BitConverter.GetBytes(data.Length);
#if (NET35 || NET40)
            stream.Write(size, 0, data.Length);
            stream.Write(data, 0, data.Length);
#else
            return stream.WriteAsync(size, 0, data.Length).ContinueWith((t) => stream.WriteAsync(data, 0, data.Length));
#endif
        }

        /// <summary>
        /// read one byte from server
        /// </summary>
        /// <param name="stream">stream to read</param>
        /// <param name="compress">compress mode</param>
        /// <param name="maximum">maximum read</param>
        /// <param name="isWebSocket">if reading socket is websocket</param>
        /// <returns></returns>
#if (NET35 || NET40)
        public virtual byte ReadOneByte(PipeNetworkStream stream)
        {
            return stream.ReadOneByte();
        }
#else
        public virtual Task<byte> ReadOneByteAsync(PipeNetworkStream stream)
        {
            return stream.ReadOneByteAcync();
        }
#endif


#if (NET35 || NET40)
        public virtual byte[] ReadBlockSize(PipeNetworkStream stream, int count)
#else
        public virtual async Task<byte[]> ReadBlockSizeAsync(PipeNetworkStream stream, int count)
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
        public virtual void WriteToStream(PipeNetworkStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }
#else
        public virtual Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data)
        {
            return stream.WriteAsync(data, 0, data.Length);
        }
#endif


    }
}
