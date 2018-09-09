using System;
using System.Collections.Generic;
using System.Linq;

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
        {
            byte[] encode = EncodeMessageToSend(data);
            stream.Write(encode);
        }
#else
        public override async void WriteToStream(PipeNetworkStream stream, byte[] data)
        {
            byte[] encode = EncodeMessageToSend(data);
            await stream.WriteAsync(encode);
        }
#endif

        public override byte[] ReadBlockSize(PipeNetworkStream stream, ulong count)
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
                //#if (!PORTABLE)
                //                Console.WriteLine("countToRead: " + countToRead);
                //#endif
                var readBytes = stream.Read((int)countToRead, out int readCount);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected: " + readCount);
                lengthReaded += (ulong)readCount;
                bytes.AddRange(readBytes.ToList().GetRange(0, readCount));
            }
            return bytes.ToArray();
        }

        public override byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, uint maximum)
        {
            ulong size = 0;
            List<byte> bytes = GetLengthOfWebSocket(stream, ref size);
            byte[] newBytes = ReadBlockSize(stream, size);
            List<byte> b = new List<byte>();
            b.AddRange(bytes);
            b.AddRange(newBytes);
            byte[] decode = DecodeMessage(b.ToArray());
            if (decode == null || decode.Length == 0)
            {
                throw new Exception("websocket closed by client");
            }
            return decode;
        }

        public override byte ReadOneByte(PipeNetworkStream stream)
        {
            byte[] dataBytes = ReadBlockToEnd(stream, CompressMode.None, 1);
            return dataBytes[0];
        }

        private List<byte> GetLengthOfWebSocket(PipeNetworkStream stream, ref ulong len)
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(ReadBlockSize(stream, 2));
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
                len = (ulong)bytes[1] - 128;
                len += 4;
            }
            else if (bytes[1] - 128 == 126)
            {
                byte[] newSize = ReadBlockSize(stream, 2);
                bytes.AddRange(newSize);
                len = BitConverter.ToUInt16(new byte[2] { newSize[1], newSize[0] }, 0);
                len += 4;
            }
            else if (bytes[1] - 128 == 127)
            {
                byte[] newSize = ReadBlockSize(stream, 8);
                bytes.AddRange(newSize);
                len = BitConverter.ToUInt16(new byte[8] { bytes[7], newSize[6], newSize[5], newSize[4], newSize[3], newSize[2], newSize[1], newSize[0] }, 0);
                len += 4;
            }
            else
            {
                throw new Exception("incorrect websocket data");
            }
            return bytes;
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
