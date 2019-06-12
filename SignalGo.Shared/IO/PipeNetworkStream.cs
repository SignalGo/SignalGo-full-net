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
    /// <summary>
    /// pipe line network stream helper
    /// this class help you easy to read a line of strin data from a stream
    /// solve memory heaps and work very fast and easy
    /// this stream will not lost data like streamreader when you read line of string data
    /// </summary>
    public class PipeLineStream : IDisposable
    {
        delegate void ActionRef<T>(ref T item);
        /// <summary>
        /// read buffer from stream
        /// the action help ut to override dispose exception witout check it with if always
        /// </summary>
        Action ReadBufferAsyncAction { get; set; }
        ActionRef<int> ReadAsyncAction { get; set; }
        /// <summary>
        /// constructor of pipe line stream
        /// </summary>
        /// <param name="stream">stream for read and write</param>
        /// <param name="bufferToRead">buffer to read</param>
        public PipeLineStream(Stream stream, int bufferToRead = 1024)
        {
            Stream = stream;
            BufferToRead = bufferToRead;
            ReadBufferAsyncAction = ReadBufferAsync;
        }

        /// <summary>
        /// stream to read or write data on it
        /// </summary>
        private Stream Stream { get; set; }

        /// <summary>
        /// default buffer read count from stream
        /// </summary>
        public int BufferToRead { get; set; }

        /// <summary>
        /// block of buffers readed from stream
        /// </summary>
        private ConcurrentBlockingCollection<BufferSegment> BlockBuffers { get; set; } = new ConcurrentBlockingCollection<BufferSegment>();

        /// <summary>
        /// all
        /// </summary>
        private ConcurrentQueue<BufferSegment> QueueBuffers { get; set; } = new ConcurrentQueue<BufferSegment>();

        /// <summary>
        /// write data to stream
        /// </summary>
        /// <param name="bytes">bytes of data</param>
        /// <param name="offset">offset</param>
        /// <param name="count">count to write</param>
        public void Write(ref byte[] bytes, ref int offset, ref int count)
        {
            Stream.Write(bytes, offset, count);
        }

        /// <summary>
        /// write data to stream async
        /// </summary>
        /// <param name="bytes">bytes of data</param>
        /// <param name="offset">offset</param>
        /// <param name="count">count to write</param>
        public Task WriteAsync(ref byte[] bytes, ref int offset, ref int count)
        {
            return Stream.WriteAsync(bytes, offset, count);
        }

        private async void ReadBufferAsync()
        {
            try
            {
                byte[] buffer = new byte[BufferToRead];
                int readCount = await Stream.ReadAsync(buffer, 0, buffer.Length);
                if (readCount <= 0)
                {
                    await BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
                    await BlockBuffers.CancelAsync();
                    ReadBufferAsyncAction = EmptyMethod;
                    return;
                }
                else if (readCount != buffer.Length)
                {
                    Array.Resize(ref buffer, readCount);
                }

                await BlockBuffers.AddAsync(new BufferSegment()
                {
                    Buffer = buffer,
                    Position = 0
                });
            }
            catch
            {
                await BlockBuffers.AddAsync(new BufferSegment() { Buffer = null, Position = 0 });
                await BlockBuffers.CancelAsync();
                ReadBufferAsyncAction = EmptyMethod;
            }
        }


        private void ReadBuffer()
        {
            try
            {
                byte[] buffer = new byte[BufferToRead];
                int readCount = Stream.Read(buffer, 0, buffer.Length);
                if (readCount <= 0)
                {
                    BlockBuffers.Add(new BufferSegment() { Buffer = null, Position = 0 });
                    BlockBuffers.Cancel();
                    ReadBufferAsyncAction = EmptyMethod;
                    return;
                }
                else if (readCount != buffer.Length)
                {
                    Array.Resize(ref buffer, readCount);
                }
                BlockBuffers.Add(new BufferSegment() { Buffer = buffer, Position = 0 });
            }
            catch (Exception ex)
            {
                BlockBuffers.Add(new BufferSegment() { Buffer = null, Position = 0 });
                BlockBuffers.Cancel();
                ReadBufferAsyncAction = EmptyMethod;
            }
        }

        private async Task<int> ReadAsync(byte[] bytes, int count)
        {
            if (IsClosed && QueueBuffers.IsEmpty && BlockBuffers.Count == 0)
                throw new Exception("read zero buffer! client disconnected");
            BufferSegment result;
            if (QueueBuffers.IsEmpty)
            {
                if (BlockBuffers.Count == 0)
                    ReadBufferAsyncAction();

                result = await BlockBuffers.TakeAsync();
                if (BlockBuffers.Count == 0)
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
                var readBytes = result.ReadBufferSegment(ref count);
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                Array.Copy(readBytes, bytes, readBytes.Length);
                return readBytes.Length;
            }
        }

        public int Read(byte[] bytes, int count)
        {
            BufferSegment result;
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
                var readBytes = result.ReadBufferSegment(count, out int readCount);
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                Array.Copy(readBytes.ToArray(), bytes, readCount);
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
        /// <summary>
        /// this method help us to override actions without check it with if always
        /// </summary>
        void EmptyMethod()
        {

        }

        public void Dispose()
        {
            System.IO.Stream.Dispose();
        }
    }
}
