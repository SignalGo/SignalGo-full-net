using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if (!PORTABLE)
using System.Net.Sockets;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class CustomStreamReader : Stream
    {
#if (!PORTABLE)
        NetworkStream CurrentStream { get; set; }

        public CustomStreamReader(NetworkStream currentStream)
        {
            CurrentStream = currentStream;
        }
#else
        Stream CurrentStream { get; set; }

        public CustomStreamReader(Stream currentStream)
        {
            CurrentStream = currentStream;
        }
#endif
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

        public string Line { get; set; }

#if (NET35 || NET40)
        public string ReadLine()
#else
        public async Task<string> ReadLine()
#endif
        {
            List<byte> result = new List<byte>();
#if (!PORTABLE)
            bool isFirst = true;
#endif
            do
            {
#if (!PORTABLE)
                if (!CheckDataAvalable(isFirst))
                    break;
                isFirst = false;
#endif
#if (NET35 || NET40)
                byte data = SignalGoStreamBase.CurrentBase.ReadOneByte(CurrentStream, CompressMode.None, 0);
#else
                byte data = await SignalGoStreamBase.CurrentBase.ReadOneByteAsync(CurrentStream);
#endif
                LastByteRead = data;
                result.Add((byte)data);
                if (data == 13)
                {
#if (!PORTABLE)
                    if (!CheckDataAvalable(isFirst))
                        break;
#endif
#if (NET35 || NET40)
                    data = SignalGoStreamBase.CurrentBase.ReadOneByte(CurrentStream, CompressMode.None, 0);
#else
                    data = await SignalGoStreamBase.CurrentBase.ReadOneByteAsync(CurrentStream);
#endif
                    LastByteRead = data;

                    result.Add((byte)data);
                    if (data == 10)
                        break;
                }
            }
            while (true);
            LastBytesReaded = result.ToArray();
            Line = Encoding.UTF8.GetString(LastBytesReaded, 0, LastBytesReaded.Length);
            return Line;
        }
#if (!PORTABLE)
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
#endif
    }
}
