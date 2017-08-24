using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SignalGo.Shared.IO
{
    public class CustomStreamReader : Stream
    {
        NetworkStream CurrentStream { get; set; }

        public CustomStreamReader(NetworkStream currentStream)
        {
            CurrentStream = currentStream;
        }

        public int LastByteRead { get; set; } = -5;

        public override bool CanRead => CurrentStream.CanRead;

        public override bool CanSeek => CurrentStream.CanSeek;

        public override bool CanWrite => CurrentStream.CanWrite;

        public override long Length => CurrentStream.Length;

        public override long Position { get => CurrentStream.Position; set => CurrentStream.Position = value; }

        public override void Flush()
        {
            CurrentStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return CurrentStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return CurrentStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CurrentStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CurrentStream.Write(buffer, offset, count);
        }

        public string ReadLine()
        {
            List<byte> result = new List<byte>();
            bool isFirst = true;
            do
            {
                if (!CheckDataAvalable(isFirst))
                    break;
                isFirst = false;
                int data = CurrentStream.ReadByte();
                LastByteRead = data;
                if (data == -1)
                    break;
                result.Add((byte)data);
                if (data == 13)
                {
                    if (!CheckDataAvalable(isFirst))
                        break;
                    data = CurrentStream.ReadByte();
                    LastByteRead = data;
                    if (data == -1)
                        break;
                    result.Add((byte)data);
                    if (data == 10)
                        break;
                }
            }
            while (true);
            return Encoding.UTF8.GetString(result.ToArray());
        }

        bool CheckDataAvalable(bool isFirstCall)
        {
            if (isFirstCall || CurrentStream.DataAvailable)
                return true;
            for (int i = 0; i < 50; i++)
            {
                if (!CurrentStream.DataAvailable)
                    Thread.Sleep(100);
                else
                    break;
            }
            return CurrentStream.DataAvailable;
        }
    }
}
