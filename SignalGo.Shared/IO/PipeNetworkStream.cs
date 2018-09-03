using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class PipeNetworkStream : IDisposable
    {
        private IStream Stream { get; set; }
        private int BufferToRead { get; set; }
        public PipeNetworkStream(IStream stream, int bufferToRead = 512)
        {
            Stream = stream;
            BufferToRead = bufferToRead;
        }
        public bool IsClosed { get; set; } = false;

        private BlockingCollection<BufferSegment> BlockBuffers = new BlockingCollection<BufferSegment>();
        private ConcurrentQueue<BufferSegment> QueueBuffers = new ConcurrentQueue<BufferSegment>();

#if (NET35 || NET40)

#else
        public Task WriteAsync(byte[] data)
        {
            return Stream.WriteAsync(data, 0, data.Length);
        }
#endif
        public void Write(byte[] data)
        {
            Stream.Write(data, 0, data.Length);
        }

        private bool IsWaitToRead { get; set; } = false;
#if (NET35 || NET40)
        private void ReadBuffer()
#else
        private async void ReadBuffer()
#endif
        {
            try
            {
                if (IsWaitToRead)
                    return;
                IsWaitToRead = true;
                byte[] buffer = new byte[BufferToRead];
#if (NET35 || NET40)
                int readCount = Stream.Read(buffer, 0, buffer.Length);
#else
                int readCount = await Stream.ReadAsync(buffer, 0, buffer.Length);
#endif
                if (readCount == 0)
                {
                    IsClosed = true;
                    IsWaitToRead = false;
                    BlockBuffers.Add(new BufferSegment() { Buffer = null, Position = 0 });
                    return;
                    //throw new Exception("read zero buffer! client disconnected: " + readCount);
                }
                else if (readCount != buffer.Length)
                {
                    Array.Resize(ref buffer, readCount);
                }
                IsWaitToRead = false;
                BlockBuffers.Add(new BufferSegment() { Buffer = buffer, Position = 0 });
            }
            catch
            {
                IsClosed = true;
                IsWaitToRead = false;
                BlockBuffers.Add(new BufferSegment() { Buffer = null, Position = 0 });
            }
        }

        public byte[] Read(int count, out int readCount)
        {
            if (IsClosed)
                throw new Exception("read zero buffer! client disconnected");
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                ReadBuffer();
                result = BlockBuffers.Take();
                if (IsClosed)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return Read(count, out readCount);
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return Read(count, out readCount);
            }
            else
            {
                byte[] bytes = result.ReadBufferSegment(count, out readCount);
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                return bytes;
            }
        }

        public byte[] Read(byte[] exitBytes)
        {
            if (IsClosed)
                throw new Exception("read zero buffer! client disconnected");
            List<byte> bytes = new List<byte>();
            while (true)
            {
                BufferSegment result = null;
                if (QueueBuffers.IsEmpty)
                {
                    ReadBuffer();
                    result = BlockBuffers.Take();
                    if (IsClosed)
                        throw new Exception("read zero buffer! client disconnected");
                    QueueBuffers.Enqueue(result);
                }
                else
                {
                    if (!QueueBuffers.TryPeek(out result))
                        return Read(exitBytes);
                }

                if (result.IsFinished)
                {
                    QueueBuffers.TryDequeue(out result);
                    return Read(exitBytes);
                }
                else
                {
                    if (bytes.Count > 0 && bytes.Last() == exitBytes.First() && result.WhatIsFirstByte() == exitBytes.Last())
                        exitBytes = new byte[] { exitBytes.Last() };
                    bytes.AddRange(result.Read(exitBytes, out bool isFound));
                    if (result.IsFinished)
                        QueueBuffers.TryDequeue(out result);
                    if (isFound)
                        break;
                }
            }
            return bytes.ToArray();
        }

        public byte ReadOneByte()
        {
            if (IsClosed)
                throw new Exception("read zero buffer! client disconnected");
            ReadBuffer();
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                ReadBuffer();
                result = BlockBuffers.Take();
                if (IsClosed)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return ReadOneByte();
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return ReadOneByte();
            }
            else
            {
                byte b = result.ReadFirstByte();
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                return b;
            }
        }

        public byte[] FirstLineBytes { get; set; }
        public string ReadLine()
        {
            if (IsClosed)
                throw new Exception("read zero buffer! client disconnected");
            ReadBuffer();
            FirstLineBytes = Read(new byte[] { 13, 10 });
            return Encoding.ASCII.GetString(FirstLineBytes);
        }

        public static PipeNetworkStream GetPipeNetworkStream(Stream stream)
        {
            return new PipeNetworkStream(new NormalStream(stream));
        }


        public static implicit operator PipeNetworkStream(Stream stream)
        {
            return new PipeNetworkStream(new NormalStream(stream));
        }

        public static implicit operator Stream(PipeNetworkStream pipeNetworkStream)
        {
            return new PipeNormalStream(pipeNetworkStream);
        }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}
