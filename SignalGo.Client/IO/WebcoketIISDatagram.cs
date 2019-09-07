using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Client.IO
{
    public class WebcoketIISDatagram : WebcoketDatagram
    {
        public override byte[] Dencode(byte[] bytes)
        {
            byte secondByte = bytes[1];
            int dataLength = secondByte & 127;
            int indexFirstMask = 2;
            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;


            byte[] decoded = new byte[bytes.Length - indexFirstMask];
            for (int i = indexFirstMask, j = 0; i < bytes.Length; i++, j++)
            {
                decoded[j] = bytes[i];
            }

            return decoded;
        }

        public override int GetLength(byte[] bytes)
        {
            //var length = bytes[1];
            //int maskLength = 4;
            //if (length < 126)
            //{
            //    return (2 + maskLength + length)- bytes.Length;
            //}
            //else
            //{
            //    return (4 + maskLength + length) - bytes.Length;
            //}
            int len = bytes[1];

            if (len > 125)
            {
                int a = bytes[2];
                int b = bytes[3];
                len = (a << 8) + b;
            }
            else
                len += 4;
            return len;
        }
    }
}
