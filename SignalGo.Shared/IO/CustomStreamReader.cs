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
    public class CustomStreamReader : Stream
    {

        Stream CurrentStream { get; set; }
#if (!PORTABLE)
        NetworkStream NetworkStream { get; set; }
#endif

        public CustomStreamReader(Stream currentStream)
        {
            CurrentStream = currentStream;
#if (!PORTABLE)
            if (currentStream is NetworkStream)
            {
                NetworkStream = currentStream as NetworkStream;
            }
#endif
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

        public byte[] LastBytesReaded { get; set; }
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
            LastBytesReaded = result.ToArray();
            return Encoding.UTF8.GetString(LastBytesReaded, 0, LastBytesReaded.Length);
        }
        bool CheckDataAvalable(bool isFirstCall)
        {
#if (!PORTABLE)
            if (NetworkStream == null)
                return true;
            if (isFirstCall || NetworkStream.DataAvailable)
                return true;
            for (int i = 0; i < 50; i++)
            {
                if (!NetworkStream.DataAvailable)
                    Thread.Sleep(100);
                else
                    break;
            }
            return NetworkStream.DataAvailable;
#else
            return true;
#endif
        }
    }
}
