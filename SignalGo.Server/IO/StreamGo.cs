using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SignalGo.Server.IO
{
    internal class StreamGo : Stream
    {
        NetworkStream CurrentStream { get; set; }

        public StreamGo(NetworkStream currentStream)
        {
            CurrentStream = currentStream;
        }

        long _Length;
        uint BoundarySize { get; set; }

        long _Position;
        bool IsReadFinishedBytes { get; set; } = false;

        public override bool CanRead => CurrentStream.CanRead;

        public override bool CanSeek => CurrentStream.CanSeek;

        public override bool CanWrite => CurrentStream.CanWrite;

        public override long Length => _Length;

        public override long Position { get => _Position; set => _Position = value; }

        public override void Flush()
        {
            CurrentStream.Flush();
        }

        /// <summary>
        /// Set the lenth of stream
        /// </summary>
        /// <param name="value"></param>
        public void SetOfStreamLength(long length, int _BoundarySize)
        {
            _Length = length - _BoundarySize;
            BoundarySize = (uint)_BoundarySize;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsReadFinishedBytes)
                return -1;
            if (count + Position > Length)
            {
                IsReadFinishedBytes = true;
                //Console.WriteLine("length:" + Length + "pos:" + Position);
                //Console.WriteLine("need take:" + (Length - Position + BoundarySize));
#if (!PORTABLE)
                var endBuffer = GoStreamReader.ReadBlockSize(CurrentStream, (ulong)(Length - Position + BoundarySize));
#else
                var endBuffer = GoStreamReader.ReadBlockSize(CurrentStream, (ulong)(Length - Position + BoundarySize));
#endif
                //Console.WriteLine("sizeTake:" + endBuffer.Length);
                if (endBuffer.Length == 0)
                    return 0;
                var needRead = (int)BoundarySize;
                //Console.WriteLine(endBuffer.Length + "&" + (endBuffer.Length - needRead) + " & " + needRead);
                var text = Encoding.UTF8.GetString(endBuffer.ToList().GetRange(endBuffer.Length - needRead, needRead).ToArray());
                int lineLen = 0;
                if (!text.StartsWith("\r\n"))
                {
                    lineLen = 2;
                    _Length -= 2;
                }
                //Console.WriteLine("ok&" + (endBuffer.Length - needRead - lineLen));
                var newBuffer = endBuffer.ToList().GetRange(0, endBuffer.Length - needRead - lineLen);
                if (newBuffer.Count == 0)
                    return -1;
                for (int i = 0; i < newBuffer.Count; i++)
                {
                    buffer[i] = newBuffer[i];
                }
                return newBuffer.Count;
            }
            if (count + Position > Length)
            {
                count = (int)(Length - Position);
                if (count <= 0)
                {
                    FinishRead();
                    return -1;
                }
            }
            var readCount = CurrentStream.Read(buffer, offset, count);
            Position += readCount;
            if (Position == Length)
                FinishRead();
            return readCount;
        }

        void FinishRead()
        {
            if (!IsReadFinishedBytes)
            {
                GoStreamReader.ReadBlockSize(CurrentStream, BoundarySize);
            }
            IsReadFinishedBytes = true;
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
    }
}
