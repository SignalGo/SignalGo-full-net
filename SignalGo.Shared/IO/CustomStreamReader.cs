using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Text;
using System.Threading;

namespace SignalGo.Shared.IO
{
    public class CustomStreamReader
    {
        Socket CurrentSocket { get; set; }

        public CustomStreamReader(Socket socket)
        {
            CurrentSocket = socket;
        }

        public int LastByteRead { get; set; } = -5;



        public byte[] LastBytesReaded { get; set; }
        public string ReadLine()
        {
            List<byte> result = new List<byte>();
            bool isFirst = true;
            byte[] buffer = new byte[1];
            do
            {
                if (!CheckDataAvalable(isFirst))
                    break;
                isFirst = false;
                var readCount = CurrentSocket.Receive(buffer);
                LastByteRead = buffer[0];
                if (readCount == 0)
                    break;
                result.Add(buffer[0]);
                if (buffer[0] == 13)
                {
                    if (!CheckDataAvalable(isFirst))
                        break;
                    readCount = CurrentSocket.Receive(buffer);
                    LastByteRead = buffer[0];
                    if (readCount == 0)
                        break;
                    result.Add(buffer[0]);
                    if (buffer[0] == 10)
                        break;
                }
            }
            while (true);
            LastBytesReaded = result.ToArray();
            return Encoding.UTF8.GetString(LastBytesReaded, 0, LastBytesReaded.Length);
        }

        bool CheckDataAvalable(bool isFirstCall)
        {
            if (isFirstCall || CurrentSocket.Available > 0)
                return true;
            for (int i = 0; i < 50; i++)
            {
                if (CurrentSocket.Available <= 0)
                    Thread.Sleep(100);
                else
                    break;
            }
            return CurrentSocket.Available > 0;
        }
    }
}
