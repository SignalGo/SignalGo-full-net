using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace SignalGo.Shared.Helpers
{
    public class SignalGoStreamHelper
    {
        public static byte ReadOneByte(Stream stream, TimeSpan timeout)
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            resetEvent.Reset();
            byte oneByte = 0;
            AsyncActions.Run(() =>
            {
                int data = stream.ReadByte();
                if (data >= 0)
                {
                    oneByte = (byte)data;
                    resetEvent.Set();
                }
            });

            if (resetEvent.WaitOne(timeout))
                return oneByte;
            throw new TimeoutException();
        }



        public static byte[] ReadBlockSizeDataAvalable(NetworkStream stream, ulong count)
        {
            List<byte> bytes = new List<byte>();
            ulong lengthReaded = 0;

            while (lengthReaded < count)
            {
                if (!stream.DataAvailable)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(1000);
                        if (stream.DataAvailable)
                            break;
                    }
                    if (!stream.DataAvailable)
                        break;
                }
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

    }
}
