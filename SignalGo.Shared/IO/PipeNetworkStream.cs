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
        public int BufferToRead { get; set; }
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

        public void Write(byte[] data, int offset, int count)
        {
            Stream.Write(data, offset, count);
        }

#if (!NET35 && !NET40)
        public Task WriteAsync(byte[] data, int offset, int count)
        {
            return Stream.WriteAsync(data, offset, count);
        }
#endif
        private bool IsWaitToRead { get; set; } = false;

        private readonly SemaphoreSlim lockWaitToRead = new SemaphoreSlim(1, 1);
#if (!NET35 && !NET40)
        /// <summary>
        /// read the stream buffer async
        /// </summary>
        private async void ReadBufferAsync()
        {
            try
            {
                try
                {
                    await lockWaitToRead.WaitAsync();

                    if (IsWaitToRead || IsClosed)
                        return;
                    IsWaitToRead = true;
                }
                finally
                {
                    lockWaitToRead.Release();
                }
                byte[] buffer = new byte[BufferToRead];
                // this line will write in console on every stream buffer read 
                // and it slows down the performance a bit (Just In Debug Mode Because of the frequent writing)
                //System.Diagnostics.Debug.WriteLine($"try to ReadAsync {buffer.Length}");
                int readCount = await Stream.ReadAsync(buffer, 0, buffer.Length);
                // this line will write in console on every stream buffer read 
                // and it slows down the performance a bit (Just In Debug Mode Because of the frequent writing)
                //System.Diagnostics.Debug.WriteLine($"done ReadAsync {buffer.Length}");
                if (readCount <= 0)
                {
                    IsClosed = true;
                    IsWaitToRead = false;
                    await BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
                    await BlockBuffers.CancelAsync();
                    return;
                    //throw new Exception("read zero buffer! client disconnected: " + readCount);
                }
                else if (readCount != buffer.Length)
                {
                    Array.Resize(ref buffer, readCount);
                }
                IsWaitToRead = false;
                await BlockBuffers.AddAsync(new BufferSegment() { Buffer = buffer, Position = 0 });
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                IsClosed = true;
                IsWaitToRead = false;
                await BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
                await BlockBuffers.CancelAsync();
            }
        }
#endif


        private void ReadBuffer()
        {
            try
            {
                try
                {
                    lockWaitToRead.Wait();
                    if (IsWaitToRead || IsClosed)
                        return;
                    IsWaitToRead = true;
                }
                finally
                {
                    lockWaitToRead.Release();
                }
                byte[] buffer = new byte[BufferToRead];
                System.Diagnostics.Debug.WriteLine($"try to ReadAsync {buffer.Length}");
                int readCount = Stream.Read(buffer, 0, buffer.Length);
                System.Diagnostics.Debug.WriteLine($"done ReadAsync {buffer.Length}");
                if (readCount <= 0)
                {
                    IsClosed = true;
                    IsWaitToRead = false;
                    BlockBuffers.Add(new BufferSegment() { Buffer = null, Position = 0 });
                    BlockBuffers.Cancel();
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
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                IsClosed = true;
                IsWaitToRead = false;
                BlockBuffers.Add(new BufferSegment() { Buffer = null, Position = 0 });
                BlockBuffers.Cancel();
            }
        }

#if (!NET35 && !NET40)
        public async Task<int> ReadAsync(byte[] bytes, int count)
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            if (bytes != null && bytes.Length < count)
                throw new Exception("count size is greater than bytes.length");
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                if (BlockBuffers.Count == 0)
                    ReadBufferAsync();


                result = await BlockBuffers.TakeAsync();
                if (IsClosed && BlockBuffers.Count == 0)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return await ReadAsync(bytes, count);
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return await ReadAsync(bytes, count);
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
#endif
        public int Read(byte[] bytes, int count)
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

                result = BlockBuffers.Take();
                if (IsClosed && BlockBuffers.Count == 0)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return Read(bytes, count);
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return Read(bytes, count);
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

#if (!NET35 && !NET40)
        public async Task<byte[]> ReadAsync(byte[] exitBytes)
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
                        ReadBufferAsync();
                    result = await BlockBuffers.TakeAsync();

                    if (IsClosed && BlockBuffers.Count == 0)
                        throw new Exception("read zero buffer! client disconnected");
                    QueueBuffers.Enqueue(result);
                }
                else
                {
                    if (!QueueBuffers.TryPeek(out result))
                        return await ReadAsync(exitBytes);
                }

                if (result.IsFinished)
                {
                    QueueBuffers.TryDequeue(out result);
                    return await ReadAsync(exitBytes);
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
#endif

        public byte[] Read(byte[] exitBytes)
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

                    result = BlockBuffers.Take();

                    if (IsClosed && BlockBuffers.Count == 0)
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

#if (!NET35 && !NET40)
        public async Task<byte> ReadOneByteAsync()
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                if (BlockBuffers.Count == 0)
                    ReadBufferAsync();

                result = await BlockBuffers.TakeAsync();

                if (IsClosed && BlockBuffers.Count == 0)
                    throw new Exception("read zero buffer! client disconnected");
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return await ReadOneByteAsync();
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return await ReadOneByteAsync();
            }
            else
            {
                byte b = result.ReadFirstByte();
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                return b;
            }
        }
#endif

        public byte ReadOneByte()
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                if (BlockBuffers.Count == 0)
                    ReadBuffer();

                result = BlockBuffers.Take();

                if (IsClosed && BlockBuffers.Count == 0)
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


        public string ReadLine()
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            return Encoding.ASCII.GetString(Read(new byte[] { 13, 10 }));
        }

#if (!NET35 && !NET40)
        public async Task<string> ReadLineAsync()
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            return Encoding.ASCII.GetString(await ReadAsync(new byte[] { 13, 10 }));
        }
#endif

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
