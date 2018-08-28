using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.Shared.IO
{
    public class SignalGoStreamBase : ISignalGoStream
    {
        static SignalGoStreamBase()
        {
            CurrentBase = new SignalGoStreamBase();
        }


        public static ISignalGoStream CurrentBase { get; set; }

#if (NET35 || NET40)
        public virtual void WriteToStream(PipeNetworkStream stream, byte[] data)
        {
            stream.Write(data);
        }
#else
        public virtual async void WriteToStream(PipeNetworkStream stream, byte[] data)
        {
            await stream.WriteAsync(data);
        }
#endif

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
        public virtual byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, uint maximum)
        {
            //first 4 bytes are size of block
            byte[] dataLenByte = ReadBlockSize(stream, 4);
            //convert bytes to int
            int dataLength = BitConverter.ToInt32(dataLenByte, 0);
            if (dataLength > maximum)
                throw new Exception("dataLength is upper than maximum :" + dataLength);
            //read a block
            byte[] dataBytes = ReadBlockSize(stream, (ulong)dataLength);
            return dataBytes;
        }

        public virtual byte[] ReadBlockSize(PipeNetworkStream stream, ulong count)
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
                byte[] readBytes = stream.Read((int)countToRead, out int readCount);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += (ulong)readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }

#if (NET35 || NET40)
        public virtual void WriteBlockToStream(PipeNetworkStream stream, byte[] data)
        {
            byte[] size = BitConverter.GetBytes(data.Length);
            stream.Write(size);
            stream.Write(data);
        }
#else
        public virtual async void WriteBlockToStream(PipeNetworkStream stream, byte[] data)
        {
            byte[] size = BitConverter.GetBytes(data.Length);
            await stream.WriteAsync(size);
            await stream.WriteAsync(data);
        }
#endif
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
        //#if (NET35 || NET40)
        //        public virtual byte ReadOneByteAsync(Stream stream)
        //#else
        //        public virtual async Task<byte> ReadOneByteAsync(IStream stream)
        //#endif
        //        {
        //#if (NET35 || NET40)
        //            var data = stream.ReadByte();
        //#else
        //            byte[] bytes = new byte[1];
        //            int data = await stream.ReadAsync(bytes, 0, bytes.Length);
        //#endif
        //            if (data <= 0)
        //                throw new Exception($"read one byte is correct or disconnected client! {data}");
        //#if (NET35 || NET40)
        //            return (byte)data;
        //#else
        //            return bytes[0];
        //#endif
        //        }
    }
}
