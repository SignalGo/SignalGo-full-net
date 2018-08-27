using System;
using System.Collections.Generic;
using System.IO;
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

        public virtual void WriteToStream(System.IO.Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

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
        public virtual byte[] ReadBlockToEnd(Stream stream, CompressMode compress, uint maximum)
        {
            //first 4 bytes are size of block
            byte[] dataLenByte = ReadBlockSize(stream, 4);
            //convert bytes to int
            int dataLength = BitConverter.ToInt32(dataLenByte, 0);
            if (dataLength > maximum)
                throw new Exception("dataLength is upper than maximum :" + dataLength);
            //else
            //    AutoLogger.LogText("size: " + dataLength);
            //read a block
            byte[] dataBytes = ReadBlockSize(stream, (ulong)dataLength);
            return dataBytes;
        }

        public virtual byte[] ReadBlockSize(Stream stream, ulong count)
        {
            List<byte> bytes = new List<byte>();
            ulong lengthReaded = 0;

            while (lengthReaded < count)
            {
                ulong countToRead = count;
                if (lengthReaded + countToRead > count)
                {
                    countToRead = count - lengthReaded;
                }
                byte[] readBytes = new byte[countToRead];
                int readCount = stream.Read(readBytes, 0, (int)countToRead);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += (ulong)readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }

        public virtual void WriteBlockToStream(Stream stream, byte[] data)
        {
            byte[] size = BitConverter.GetBytes(data.Length);
            stream.Write(size, 0, size.Length);
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
        public virtual byte ReadOneByte(Stream stream)
        {
            int data = stream.ReadByte();
            if (data < 0)
                throw new Exception($"read one byte is correct or disconnected client! {data}");
            return (byte)data;
        }
#if (NET35 || NET40)
        public virtual byte ReadOneByteAsync(Stream stream)
#else
        public virtual async Task<byte> ReadOneByteAsync(Stream stream)
#endif
        {
#if (NET35 || NET40)
            var data = stream.ReadByte();
#else
            byte[] bytes = new byte[1];
            int data = await stream.ReadAsync(bytes, 0, bytes.Length);
#endif
            if (data <= 0)
                throw new Exception($"read one byte is correct or disconnected client! {data}");
#if (NET35 || NET40)
            return (byte)data;
#else
            return bytes[0];
#endif
        }
    }
}
