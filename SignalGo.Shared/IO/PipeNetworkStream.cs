// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using SignalGo.Shared.Enums;
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
        /// request headers
        /// </summary>
        public IDictionary<string, string[]> RequestHeaders { get; set; } = new Dictionary<string, string[]>();

        static readonly char[] SplitHeader = { ':' };
        /// <summary>
        /// procol type of this stream
        /// </summary>
        public ProtocolType ProtocolType { get; set; } = ProtocolType.None;
        /// <summary>
        /// encoding system of pipelines
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        /// <summary>
        /// size of buffer to read data from stream
        /// </summary>
        public int BufferSize { get; set; } = 2048;
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
        /// read a bytes array to end with a count you want to read
        /// </summary>
        public ReadToEndFunction ReadToEndFunction { get; set; }
        /// <summary>
        /// read a bytes array to end with a count you want to read async
        /// </summary>
        public ReadToEndAsyncFunction ReadToEndAsyncFunction { get; set; }
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
            ReadToEndFunction = ReadToEnd;
            ReadToEndAsyncFunction = ReadToEndAsync;
        }

        /// <summary>
        /// stream to read or write data on it
        /// </summary>
        private Stream Stream { get; set; }
        /// <summary>
        /// read one byte from Stream async
        /// </summary>
        /// <returns></returns>
        public async Task<Memory<byte>> ReadOneByteAsync()
        {
            var bytes = new Memory<byte>(new byte[1]);
            var readCount = await ReadAsyncAction(bytes);
            if (readCount <= 0)
                throw new Exception("read zero buffer! client disconnected");
            return bytes;
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
        /// read one line from server
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadLineAsync()
        {
            StringBuilder builder = new StringBuilder();
            do
            {
                var readByte = await ReadOneByteAsync();
                if (readByte.Span[0] == 13)
                {
                    readByte = await ReadOneByteAsync();
                    if (readByte.Span[0] == 10)
                        return builder.ToString();
                    builder.Append((char)readByte.Span[0]);
                }
                builder.Append((char)readByte.Span[0]);
            } while (true);
        }

        /// <summary>
        /// read all of the lines and detect headers
        /// </summary>
        /// <returns></returns>
        public async Task ReadAllLinesAsync()
        {
            //read all of the lines
            var firstLine = await ReadLineAsync();
            var firstLineText = firstLine.ToString();
            if (firstLineText.IndexOf("http/", StringComparison.OrdinalIgnoreCase) >= 0)
                ProtocolType = ProtocolType.Http;

            StringBuilder builder = new StringBuilder();
            //read one line
            //one header
            int readLength = 1024;
            bool isContinue = true;
            do
            {
                var bytes = new Memory<byte>(new byte[readLength]);
                var readCount = await ReadAsyncAction(bytes);
                if (readCount <= 0)
                    throw new Exception("read zero buffer! client disconnected");
                for (int i = 0; i < readCount; i++)
                {
                    if (bytes.Span[i] == 13)
                    {
                        if (i == readCount)
                        {
                            var oneByte = await ReadOneByteAsync();
                            if (oneByte.Span[0] == 10)
                            {
                                //last empty line readed and its done
                                if (builder.Length == 0)
                                {
                                    isContinue = false;
                                    break;
                                }
                                else
                                {
                                    CreateHeader(builder);
                                    continue;
                                }
                            }
                        }
                        else if (bytes.Span[i + 1] == 10)
                        {
                            i++;
                            //last empty line readed and its done
                            if (builder.Length == 0)
                            {
                                isContinue = false;
                                break;
                            }
                            else
                            {
                                CreateHeader(builder);
                                continue;
                            }
                        }
                    }

                    builder.Append((char)bytes.Span[i]);
                }
            } while (isContinue);


            //change protocol if its websocket
            //if (ProtocolType == ProtocolType.Http && header[0].IndexOf("", StringComparison.OrdinalIgnoreCase) >= 0)
            //    ProtocolType = ProtocolType.Websocket;
        }

        void CreateHeader(StringBuilder builder)
        {
            //header to string
            var text = builder.ToString();
            //split header
            var header = text.Split(SplitHeader, 2);
            try
            {
                RequestHeaders.Add(header[0], header[1].Split(';'));
            }
            catch (Exception ex)
            {
                //throw user friendly exception for add header exception
                if (string.IsNullOrEmpty(header[0]))
                    throw new Exception("header key is null or empty", ex);
                else if (RequestHeaders.ContainsKey(header[0]))
                    throw new Exception($"header {header[0]} is duplicate", ex);
                else
                    throw;
            }
            builder.Clear();
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
        /// read count of data
        /// </summary>
        /// <param name="count">count to read from stream</param>
        /// <returns>bytes readed from stream</returns>
        private byte[] ReadToEnd(int count)
        {
            List<byte> result = new List<byte>();
            int readedSize = 0;
            do
            {
                int remaning = count - readedSize;
                int countToRead = BufferSize > remaning ? remaning : BufferSize;
                byte[] bytes = new byte[countToRead];
                var readedCount = ReadAction(bytes, 0, countToRead);
                if (readedCount <= 0)
                    throw new Exception("read zero buffer! client disconnected");

                result.AddRange(bytes.Take(readedCount));
                readedSize += readedCount;

            } while (readedSize != count);
            return result.ToArray();
        }
        /// <summary>
        /// read count of data
        /// </summary>
        /// <param name="count">count to read from stream</param>
        /// <returns>bytes readed from stream</returns>
        private async Task<byte[]> ReadToEndAsync(int count)
        {
            List<byte> result = new List<byte>();
            int readedSize = 0;
            do
            {
                int remaning = count - readedSize;
                int countToRead = BufferSize > remaning ? remaning : BufferSize;
                byte[] bytes = new byte[countToRead];
                var readedCount = await ReadAsyncAction(bytes);
                if (readedCount <= 0)
                    throw new Exception("read zero buffer! client disconnected");

                result.AddRange(bytes.Take(readedCount));
                readedSize += readedCount;

            } while (readedSize != count);
            return result.ToArray();
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
