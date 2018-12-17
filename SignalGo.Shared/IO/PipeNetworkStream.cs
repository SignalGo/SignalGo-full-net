using SignalGo.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public class PipeNetworkStream : IDisposable
    {
        private IStream Stream { get; set; }
        private int BufferToRead { get; set; }
        public int Timeout { get; set; } = -1;
        public PipeNetworkStream(IStream stream, int timeout = -1, int bufferToRead = 512)
        {
            Timeout = timeout;
            Stream = stream;
            BufferToRead = bufferToRead;
            BlockBuffers = new ConcurrentBlockingCollection<BufferSegment>();
            //BlockBuffers = new BlockingCollection<BufferSegment>();
        }


        public bool IsClosed { get; set; } = false;

        //private BlockingCollection<BufferSegment> BlockBuffers { get; set; }
        private ConcurrentBlockingCollection<BufferSegment> BlockBuffers { get; set; }
        private ConcurrentQueue<BufferSegment> QueueBuffers = new ConcurrentQueue<BufferSegment>();

#if (NET35 || NET40)
        public void Write(byte[] data, int offset, int count)
        {
            Stream.Write(data, offset, count);
        }
#else
        public Task WriteAsync(byte[] data, int offset, int count)
        {
            return Stream.WriteAsync(data, offset, count);
        }
#endif
        private bool IsWaitToRead { get; set; } = false;

        private readonly SemaphoreSlim lockWaitToRead = new SemaphoreSlim(1, 1);
#if (NET35 || NET40)
        private void ReadBuffer()
#else
        private async void ReadBuffer()
#endif
        {
            try
            {
                try
                {
#if (NET35 || NET40)
                    lockWaitToRead.Wait();
#else
                    await lockWaitToRead.WaitAsync();
#endif
                    if (IsWaitToRead || IsClosed)
                        return;
                    IsWaitToRead = true;
                }
                finally
                {
                    lockWaitToRead.Release();
                }
                byte[] buffer = new byte[BufferToRead];

#if (NET35 || NET40)
                int readCount = Stream.Read(buffer, 0, buffer.Length);
#else
                int readCount = await Stream.ReadAsync(buffer, 0, buffer.Length);
#endif
                if (readCount <= 0)
                {
                    IsClosed = true;
                    IsWaitToRead = false;
#if (NET35 || NET40)
                    BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
#else
                    await BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
#endif

#if (NET35 || NET40)
                    BlockBuffers.CancelAsync();
#else
                    await BlockBuffers.CancelAsync();
#endif
                    return;
                    //throw new Exception("read zero buffer! client disconnected: " + readCount);
                }
                else if (readCount != buffer.Length)
                {
                    Array.Resize(ref buffer, readCount);
                }
                IsWaitToRead = false;
#if (NET35 || NET40)
                BlockBuffers.AddAsync(new BufferSegment() { Buffer = buffer, Position = 0 });
#else
                await BlockBuffers.AddAsync(new BufferSegment() { Buffer = buffer, Position = 0 });
#endif
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                IsClosed = true;
                IsWaitToRead = false;
#if (NET35 || NET40)
                BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
#else
                await BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
#endif
#if (NET35 || NET40)
                BlockBuffers.CancelAsync();
#else
                await BlockBuffers.CancelAsync();
#endif
            }
        }

#if (NET35 || NET40)
        public int Read(byte[] bytes, int count)
#else
        public async Task<int> ReadAsync(byte[] bytes, int count)
#endif
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            if (bytes != null && bytes.Length < count)
                throw new Exception("count size is greater than bytes.length");
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                if (BlockBuffers.Count == 0)
                    ReadBuffer();

#if (NET35 || NET40)
                result = BlockBuffers.TakeAsync();
#else
                result = await BlockBuffers.TakeAsync();
#endif
                if (IsClosed && BlockBuffers.Count == 0)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
#if (NET35 || NET40)
                    return Read(bytes, count);
#else
                    return await ReadAsync(bytes, count);
#endif
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
#if (NET35 || NET40)
                return Read(bytes, count);
#else
                return await ReadAsync(bytes, count);
#endif
            }
            else
            {
                byte[] readBytes = result.ReadBufferSegment(count, out int readCount);
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                Array.Copy(readBytes, bytes, readCount);
                return readCount;
            }
        }

#if (NET35 || NET40)
        public byte[] Read(byte[] exitBytes)
#else
        public async Task<byte[]> ReadAsync(byte[] exitBytes)
#endif
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            List<byte> bytes = new List<byte>();
            while (true)
            {
                BufferSegment result = null;
                if (QueueBuffers.IsEmpty)
                {
                    if (BlockBuffers.Count == 0)
                        ReadBuffer();

#if (NET35 || NET40)
                    result = BlockBuffers.TakeAsync();
#else
                    result = await BlockBuffers.TakeAsync();
#endif

                    if (IsClosed && BlockBuffers.Count == 0)
                        throw new Exception("read zero buffer! client disconnected");
                    QueueBuffers.Enqueue(result);
                }
                else
                {
                    if (!QueueBuffers.TryPeek(out result))
#if (NET35 || NET40)
                        return Read(exitBytes);
#else
                        return await ReadAsync(exitBytes);
#endif
                }

                if (result.IsFinished)
                {
                    QueueBuffers.TryDequeue(out result);
#if (NET35 || NET40)
                    return Read(exitBytes);
#else
                    return await ReadAsync(exitBytes);
#endif
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

#if (NET35 || NET40)
        public byte ReadOneByte()
#else
        public async Task<byte> ReadOneByteAcync()
#endif
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                if (BlockBuffers.Count == 0)
                    ReadBuffer();

#if (NET35 || NET40)
                result = BlockBuffers.TakeAsync();
#else
                result = await BlockBuffers.TakeAsync();
#endif

                if (IsClosed && BlockBuffers.Count == 0)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
#if (NET35 || NET40)
                    return ReadOneByte();
#else
                    return await ReadOneByteAcync();
#endif
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
#if (NET35 || NET40)
                return ReadOneByte();
#else
                return await ReadOneByteAcync();
#endif
            }
            else
            {
                byte b = result.ReadFirstByte();
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                return b;
            }
        }

#if (NET35 || NET40)
        public string ReadLine()
#else
        public async Task<string> ReadLineAsync()
#endif
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
#if (NET35 || NET40)
            return Encoding.ASCII.GetString(Read(new byte[] { 13, 10 }));
#else
            return Encoding.ASCII.GetString(await ReadAsync(new byte[] { 13, 10 }));
#endif
        }

#if (NET35 || NET40)
        public string ReadLine(string endOfLine)
#else
        public async Task<string> ReadLineAsync(string endOfLine)
#endif
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
#if (NET35 || NET40)
            byte[] bytes = Read(Encoding.ASCII.GetBytes(endOfLine));
#else
            byte[] bytes = await ReadAsync(Encoding.ASCII.GetBytes(endOfLine));
#endif
            return Encoding.ASCII.GetString(bytes);
        }

        public static PipeNetworkStream GetPipeNetworkStream(Stream stream)
        {
            return new PipeNetworkStream(new NormalStream(stream), 0);
        }

        public static implicit operator PipeNetworkStream(Stream stream)
        {
            return new PipeNetworkStream(new NormalStream(stream), 0);
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
