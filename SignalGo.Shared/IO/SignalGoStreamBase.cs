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

#if (!NET35 &&!NET40)
        public virtual async Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, CompressMode compress, int maximum)
        {
            //first 4 bytes are size of block
            byte[] dataLenByte = await ReadBlockSizeAsync(stream, 4);
            //convert bytes to int
            int dataLength = BitConverter.ToInt32(dataLenByte, 0);
            if (dataLength > maximum)
                throw new Exception("dataLength is upper than maximum :" + dataLength);
            //read a block
            byte[] dataBytes = await ReadBlockSizeAsync(stream, dataLength);
            return dataBytes;
        }
#endif

        public virtual byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum)
        {
            //first 4 bytes are size of block
            byte[] dataLenByte = ReadBlockSize(stream, 4);
            //convert bytes to int
            int dataLength = BitConverter.ToInt32(dataLenByte, 0);
            if (dataLength > maximum)
                throw new Exception("dataLength is upper than maximum :" + dataLength);
            //read a block
            byte[] dataBytes = ReadBlockSize(stream, dataLength);
            return dataBytes;
        }

#if (!NET35 && !NET40)
        public Task WriteBlockToStreamAsync(PipeNetworkStream stream, byte[] data)
        {
            byte[] size = BitConverter.GetBytes(data.Length);
            return stream.WriteAsync(size, 0, size.Length).ContinueWith((t) => stream.WriteAsync(data, 0, data.Length));
        }
#endif

        public void WriteBlockToStream(PipeNetworkStream stream, byte[] data)
        {
            byte[] size = BitConverter.GetBytes(data.Length);
            stream.Write(size, 0, data.Length);
            stream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// read one byte from server
        /// </summary>
        /// <param name="stream">stream to read</param>
        /// <param name="compress">compress mode</param>
        /// <param name="maximum">maximum read</param>
        /// <param name="isWebSocket">if reading socket is websocket</param>
        /// <returns></returns>
        public virtual byte ReadOneByte(PipeNetworkStream stream)
        {
            return stream.ReadOneByte();
        }

#if (!NET35 && !NET40)
        public virtual Task<byte> ReadOneByteAsync(PipeNetworkStream stream)
        {
            return stream.ReadOneByteAcync();
        }
#endif


#if (!NET35 && !NET40)
        public virtual async Task<byte[]> ReadBlockSizeAsync(PipeNetworkStream stream, int count)
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
                int readCount = await stream.ReadAsync(readBytes, countToRead);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }
#endif

        public virtual byte[] ReadBlockSize(PipeNetworkStream stream, int count)
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
                int readCount = stream.Read(readBytes, countToRead);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }

        public virtual void WriteToStream(PipeNetworkStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }
#if (!NET35 && !NET40)
        public virtual Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data)
        {
            return stream.WriteAsync(data, 0, data.Length);
        }
#endif


    }
}
