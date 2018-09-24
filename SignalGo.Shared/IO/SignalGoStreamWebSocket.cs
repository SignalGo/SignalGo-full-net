using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class SignalGoStreamWebSocket : SignalGoStreamBase
    {
        static SignalGoStreamWebSocket()
        {
            CurrentWebSocket = new SignalGoStreamWebSocket();
        }

        public static ISignalGoStream CurrentWebSocket { get; set; }


#if (NET35 || NET40)
        public override void WriteToStream(PipeNetworkStream stream, byte[] data)
#else
        public override Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data)
#endif
        {
            byte[] encode = EncodeMessageToSend(data);
#if (NET35 || NET40)
            stream.Write(encode, 0, encode.Length);
#else
            return stream.WriteAsync(encode, 0, encode.Length);
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
                int readCount =  stream.Read(readBytes, countToRead);
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
        public override byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum)
#else
        public override async Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, CompressMode compress, int maximum)
#endif
        {
#if (NET35 || NET40)
            Tuple<int, byte[]> data = GetLengthOfWebSocket(stream);
#else
            Tuple<int, byte[]> data = await GetLengthOfWebSocketAsync(stream);
#endif
            byte[] buffer = data.Item2;
#if (NET35 || NET40)
            byte[] newBytes = ReadBlockSize(stream, data.Item1);
#else
            byte[] newBytes = await ReadBlockSizeAsync(stream, data.Item1);
#endif
            List<byte> b = new List<byte>();
            b.AddRange(buffer);
            b.AddRange(newBytes);
            byte[] decode = DecodeMessage(b.ToArray());
            if (decode == null || decode.Length == 0)
            {
                throw new Exception("websocket closed by client");
            }
            return decode;
        }

#if (NET35 || NET40)
        public override byte ReadOneByte(PipeNetworkStream stream)
        {
            byte[] dataBytes = ReadBlockToEnd(stream, CompressMode.None, 1);
            return dataBytes[0];
        }
#else
        public override async Task<byte> ReadOneByteAsync(PipeNetworkStream stream)
        {
            byte[] dataBytes = await ReadBlockToEndAsync(stream, CompressMode.None, 1);
            return dataBytes[0];
        }
#endif

#if (NET35 || NET40)
        private Tuple<int, byte[]> GetLengthOfWebSocket(PipeNetworkStream stream)
#else
        private async Task<Tuple<int, byte[]>> GetLengthOfWebSocketAsync(PipeNetworkStream stream)
#endif
        {
            List<byte> bytes = new List<byte>();
            int len = 0;
#if (NET35 || NET40)
            bytes.AddRange(ReadBlockSize(stream, 2));
#else
            bytes.AddRange(await ReadBlockSizeAsync(stream, 2));
#endif
            //#if (!PORTABLE)
            //            Console.WriteLine("read block bytes: " + bytes.Count);
            //            foreach (var item in bytes)
            //            {
            //                Console.WriteLine(item);
            //            }
            //            Console.WriteLine("end read block bytes");
            //#endif

            if (bytes[1] - 128 <= 125)
            {
                len = bytes[1] - 128;
                len += 4;
            }
            else if (bytes[1] - 128 == 126)
            {
#if (NET35 || NET40)
                byte[] newSize = ReadBlockSize(stream, 2);
#else
                byte[] newSize = await ReadBlockSizeAsync(stream, 2);
#endif
                bytes.AddRange(newSize);
                len = BitConverter.ToUInt16(new byte[2] { newSize[1], newSize[0] }, 0);
                len += 4;
            }
            else if (bytes[1] - 128 == 127)
            {
#if (NET35 || NET40)
                byte[] newSize = ReadBlockSize(stream, 8);
#else
                byte[] newSize = await ReadBlockSizeAsync(stream, 8);
#endif
                bytes.AddRange(newSize);
                len = BitConverter.ToUInt16(new byte[8] { bytes[7], newSize[6], newSize[5], newSize[4], newSize[3], newSize[2], newSize[1], newSize[0] }, 0);
                len += 4;
            }
            else
            {
                throw new Exception("incorrect websocket data");
            }

            return new Tuple<int, byte[]>(len, bytes.ToArray());
        }

        public override byte[] EncodeMessageToSend(byte[] bytesRaw)
        {
            byte[] response;
            byte[] frame = new byte[10];

            int indexStartRawData = -1;
            int length = bytesRaw.Length;

            frame[0] = 129;
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = 126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = 127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            int i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        public byte[] DecodeMessage(byte[] bytes)
        {
            List<byte> ret = new List<byte>();
            int offset = 0;
            while (offset + 6 < bytes.Length)
            {
                // format: 0==ascii/binary 1=length-0x80, byte 2,3,4,5=key, 6+len=message, repeat with offset for next...
                int len = bytes[offset + 1] - 0x80;

                if (len <= 125)
                {

                    //String data = Encoding.UTF8.GetString(bytes);
                    //Debug.Log("len=" + len + "bytes[" + bytes.Length + "]=" + ByteArrayToString(bytes) + " data[" + data.Length + "]=" + data);
                    //Debug.Log("len=" + len + " offset=" + offset);
                    byte[] key = new byte[] { bytes[offset + 2], bytes[offset + 3], bytes[offset + 4], bytes[offset + 5] };
                    byte[] decoded = new byte[len];
                    for (int i = 0; i < len; i++)
                    {
                        int realPos = offset + 6 + i;
                        decoded[i] = (byte)(bytes[realPos] ^ key[i % 4]);
                    }
                    offset += 6 + len;
                    ret.AddRange(decoded);
                }
                else
                {
                    int a = bytes[offset + 2];
                    int b = bytes[offset + 3];
                    len = (a << 8) + b;
                    //Debug.Log("Length of ws: " + len);

                    byte[] key = new byte[] { bytes[offset + 4], bytes[offset + 5], bytes[offset + 6], bytes[offset + 7] };
                    byte[] decoded = new byte[len];
                    for (int i = 0; i < len; i++)
                    {
                        int realPos = offset + 8 + i;
                        decoded[i] = (byte)(bytes[realPos] ^ key[i % 4]);
                    }

                    offset += 8 + len;
                    ret.AddRange(decoded);
                }
            }
            return ret.ToArray();
        }
    }
}
