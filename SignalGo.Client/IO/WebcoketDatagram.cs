using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Client.IO
{
    public class WebcoketDatagram : WebcoketDatagramBase
    {
        public byte[] Dencode2(byte[] bytes)
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
        public override byte[] Dencode(byte[] bytes)
        {
            string incomingData = string.Empty;
            byte secondByte = bytes[1];
            int dataLength = secondByte & 127;
            int indexFirstMask = 2;
            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;

            IEnumerable<byte> keys = bytes.Skip(indexFirstMask).Take(4);
            //int indexFirstDataByte = indexFirstMask + 4;

            byte[] decoded = new byte[bytes.Length - indexFirstMask];
            for (int i = indexFirstMask, j = 0; i < bytes.Length; i++, j++)
            {
                decoded[j] = (byte)(bytes[i] ^ keys.ElementAt(j % 4));
            }

            return decoded;
        }
        //public override byte[] Dencode(byte[] bytes)
        //{
        //    byte secondByte = bytes[1];
        //    int dataLength = secondByte & 127;
        //    int indexFirstMask = 2;
        //    if (dataLength == 126)
        //        indexFirstMask = 4;
        //    else if (dataLength == 127)
        //        indexFirstMask = 10;


        //    byte[] decoded = new byte[bytes.Length - indexFirstMask];
        //    for (int i = indexFirstMask, j = 0; i < bytes.Length; i++, j++)
        //    {
        //        decoded[j] = bytes[i];
        //    }

        //    return Dencode2(decoded);
        //}

        private static Random m_Random = new Random();

        private static void GenerateMaskClient(byte[] mask, int offset)
        {
            int maxPos = Math.Min(offset + 4, mask.Length);

            for (int i = offset; i < maxPos; i++)
            {
                mask[i] = (byte)m_Random.Next(0, 255);
            }
        }

        private static void MaskDataClient(byte[] rawData, int offset, int length, byte[] outputData, int outputOffset, byte[] mask, int maskOffset)
        {
            for (int i = 0; i < length; i++)
            {
                int pos = offset + i;
                outputData[outputOffset++] = (byte)(rawData[pos] ^ mask[maskOffset + i % 4]);
            }
        }



        public override byte[] Encode(byte[] bytes)
        {
            int opCode = 2;
            bool isFinal = true;
            int offset = 0;
            byte[] fragment;
            int length = bytes.Length;
            int maskLength = 4;

            if (length < 126)
            {
                fragment = new byte[2 + maskLength + length];
                fragment[1] = (byte)length;
            }
            else if (length < 65536)
            {
                fragment = new byte[4 + maskLength + length];
                fragment[1] = 126;
                fragment[2] = (byte)(length / 256);
                fragment[3] = (byte)(length % 256);
            }
            else
            {
                fragment = new byte[10 + maskLength + length];
                fragment[1] = 127;

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    fragment[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }


            if (isFinal)//Set FIN
                fragment[0] = (byte)(opCode | 0x80);
            else
                fragment[0] = (byte)opCode;

            //Set mask bit
            fragment[1] = (byte)(fragment[1] | 0x80);

            GenerateMaskClient(fragment, fragment.Length - maskLength - length);

            if (length > 0)
                MaskDataClient(bytes, offset, length, fragment, fragment.Length - length, fragment, fragment.Length - maskLength - length);

            return fragment;
        }

        public override int GetLength(byte[] bytes)
        {
            int len = bytes[1];

            if (len > 125)
            {
                int a = bytes[2];
                int b = bytes[3];
                len = (a << 8) + b;
            }
            return len;
        }

        public override Tuple<int, byte[]> GetBlockLength(Stream stream, Func<int, byte[]> readBlockSize)
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(readBlockSize(2));
            if (bytes[1] > 125)
                bytes.AddRange(readBlockSize(2));
            var len = GetLength(bytes.ToArray());

            return new Tuple<int, byte[]>(len, bytes.ToArray());
        }
#if (!NET35 && !NET40)
        public override async Task<Tuple<int, byte[]>> GetBlockLengthAsync(Stream stream, Func<int, Task<byte[]>> readBlockSizeAsync)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(await readBlockSizeAsync(2));
            if (bytes[1] > 125)
                bytes.AddRange(await readBlockSizeAsync(2));
            var len = GetLength(bytes.ToArray());

            return new Tuple<int, byte[]>(len, bytes.ToArray());
        }
#endif
    }
}
