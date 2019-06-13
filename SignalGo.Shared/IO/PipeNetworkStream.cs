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
        /// <summary>
        /// read buffer from stream
        /// the action help us to override dispose exception without check it with if always
        /// </summary>
        public ReadAsyncFunction ReadAsyncAction { get; set; }
        /// <summary>
        /// read buffer from stream
        /// the action help us to override dispose exception without check it with if always
        /// </summary>
        public ReadFunction ReadAction { get; set; }
        /// <summary>
        /// write data to stream
        /// </summary>
        public WriteAsyncAction WriteAsyncAction { get; set; }
        /// <summary>
        /// write data to stream
        /// </summary>
        public WriteAction WriteAction { get; set; }
        
        /// <summary>
        /// flush the stream
        /// </summary>
        public Action FlushAction { get; set; }
        /// <summary>
        /// flush the stream async
        /// </summary>
        public Func<Task> FlushAsyncAction { get; set; }
        /// <summary>
        /// constructor of pipe line stream
        /// </summary>
        /// <param name="stream">stream for read and write</param>
        public PipeLineStream(Stream stream)
        {
            Stream = stream;
            ReadAsyncAction = Stream.ReadAsync;
            WriteAsyncAction = Stream.WriteAsync;
            FlushAction = Stream.Flush;
            FlushAsyncAction = Stream.FlushAsync;
            ReadAction = Stream.Read;
            WriteAction = Stream.Write;
        }

        /// <summary>
        /// stream to read or write data on it
        /// </summary>
        private Stream Stream { get; set; }
        /// <summary>
        /// read one byte from Stream async
        /// </summary>
        /// <returns></returns>
        public async Task<byte> ReadOneByteAsync()
        {
            var bytes = new byte[1];
            var readCount = await ReadAsyncAction(bytes, 0, 1);
            if (readCount <= 0)
                throw new Exception("read zero buffer! client disconnected");
            return bytes[0];
        }

        /// <summary>
        /// read one byte from Stream async
        /// </summary>
        /// <returns></returns>
        public byte ReadOneByte()
        {
            var bytes = new byte[1];
            var readCount = ReadAction(bytes, 0, 1);
            if (readCount <= 0)
                throw new Exception("read zero buffer! client disconnected");
            return bytes[0];
        }

        /// <summary>
        /// reda one line from server
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadLineAsync()
        {
            StringBuilder builder = new StringBuilder();
            do
            {
                var bytes = new byte[1];
                var readCount = await ReadAsyncAction(bytes, 0, 1);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected");
                if (bytes[0] == 13)
                {
                    readCount = await ReadAsyncAction(bytes, 0, 1);
                    if (readCount <= 0)
                        throw new Exception("read zero buffer! client disconnected");
                    if (bytes[0] == 10)
                        return builder.ToString();
                    builder.Append((char)bytes[0]);
                }
                builder.Append((char)bytes[0]);
            } while (true);
        }

        /// <summary>
        /// read one line from server
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            StringBuilder builder = new StringBuilder();
            do
            {
                var bytes = new byte[1];
                var readCount = ReadAction(bytes, 0, 1);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected");
                if (bytes[0] == 13)
                {
                    readCount = ReadAction(bytes, 0, 1);
                    if (readCount <= 0)
                        throw new Exception("read zero buffer! client disconnected");
                    if (bytes[0] == 10)
                        return builder.ToString();
                    builder.Append((char)bytes[0]);
                }
                builder.Append((char)bytes[0]);
            } while (true);
        }
        /// <summary>
        /// dispose the stream
        /// </summary>
        public void Dispose()
        {
            Stream.Dispose();
        }

    }
}
