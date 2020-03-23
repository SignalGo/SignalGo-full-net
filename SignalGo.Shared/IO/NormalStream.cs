// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    /// <summary>
    /// normal read and write stream
    /// </summary>
    public class NormalStream : IStream
    {
        PipeLineStream PipeLineStream { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeLineStream"></param>
        public NormalStream(PipeLineStream pipeLineStream)
        {
            PipeLineStream = pipeLineStream;
            FlushAction = PipeLineStream.FlushAction;
            FlushAsync = PipeLineStream.FlushAsyncAction;
            Read = PipeLineStream.ReadAction;
            ReadAsync = PipeLineStream.ReadAsyncAction;
            Write = PipeLineStream.WriteAction;
            WriteAsync = PipeLineStream.WriteAsyncAction;
            ReadOneByte = PipeLineStream.ReadOneByte;
            ReadOneByteAsync = PipeLineStream.ReadOneByteAsync;
            ReadLine = PipeLineStream.ReadLine;
            ReadLineAsync = PipeLineStream.ReadLineAsync;
            ReadToEndFunction = PipeLineStream.ReadToEndFunction;
            ReadToEndAsyncFunction = PipeLineStream.ReadToEndAsyncFunction;
        }
        /// <summary>
        /// receive data timeouts
        /// </summary>
        public int ReceiveTimeout { get; set; } = -1;
        /// <summary>
        /// send data timeouts
        /// </summary>
        public int SendTimeout { get; set; } = -1;
        /// <summary>
        /// flush data of stream
        /// </summary>
        public Action FlushAction { get; set; }
        /// <summary>
        /// flush data of stream
        /// </summary>
        public Func<Task> FlushAsync { get; set; }

        /// <summary>
        /// read data from stream
        /// </summary>
        /// <returns>readed count</returns>
        public ReadFunction Read { get; set; }
        /// <summary>
        /// read data from stream by async
        /// </summary>
        /// <returns>readed count</returns>
        public ReadAsyncFunction ReadAsync { get; set; }

        /// <summary>
        /// write data to stream
        /// </summary>
        public WriteAction Write { get; set; }

        /// <summary>
        /// write data to stream async
        /// </summary>
        public WriteAsyncAction WriteAsync { get; set; }

        /// <summary>
        /// read one byte from stream async
        /// </summary>
        /// <returns>byte readed</returns>
        public Func<Task<Memory<byte>>> ReadOneByteAsync { get; set; }
        /// <summary>
        /// read one byte from stream
        /// </summary>
        /// <returns>byte readed</returns>
        public Func<byte> ReadOneByte { get; set; }

        /// <summary>
        /// read a bytes array to end with a count you want to read
        /// </summary>
        public ReadToEndFunction ReadToEndFunction { get; set; }
        /// <summary>
        /// read a bytes array to end with a count you want to read async
        /// </summary>
        public ReadToEndAsyncFunction ReadToEndAsyncFunction { get; set; }

        /// <summary>
        /// read new line from stream
        /// </summary>
        /// <returns>line</returns>
        public Func<string> ReadLine { get; set; }

        /// <summary>
        /// read new line from stream async
        /// </summary>
        /// <returns>line</returns>
        public Func<Task<string>> ReadLineAsync { get; set; }


        /// <summary>
        /// read size of block every blocks has size,this method return size of block to read
        /// </summary>
        /// <returns>array of block size is int32</returns>
        public int ReadBlockSize()
        {
            var bytes = ReadToEndFunction(4);
            return BitConverter.ToInt32(bytes, 4);
        }

        /// <summary>
        /// read size of block every blocks has size,this method return size of block to read
        /// </summary>
        /// <returns>array of block size is int32</returns>
        public async Task<int> ReadBlockSizeAsync()
        {
            var bytes = await ReadToEndAsyncFunction(4);
            return BitConverter.ToInt32(bytes, 4);
        }
        /// <summary>
        /// read block of signalgo packet to end of packet
        /// </summary>
        /// <returns>bytes readed</returns>
        public byte[] ReadBlockToEnd()
        {
            var size = ReadBlockSize();
            return ReadToEndFunction(size);
        }
        /// <summary>
        /// read block of signalgo packet to end of packet async
        /// </summary>
        /// <returns>bytes readed</returns>
        public async Task<byte[]> ReadBlockToEndAsync()
        {
            var size = await ReadBlockSizeAsync();
            return await ReadToEndAsyncFunction(size);
        }

        /// <summary>
        /// write block of signalgo packet bytes array to stream
        /// </summary>
        /// <param name="bytes">bytes to write</param>
        public void WriteBlockToStream(byte[] bytes)
        {
            var size = BitConverter.ToInt32(bytes, 0);
            var sizeBytes = BitConverter.GetBytes(size);
            var allBytes = sizeBytes.Concat(bytes).ToArray();
            Write(allBytes, 0, allBytes.Length);
        }

        /// <summary>
        /// write block of signalgo packet bytes array to stream async
        /// </summary>
        /// <param name="bytes">bytes to write</param>
        public async Task WriteBlockToStreamAsync(byte[] bytes)
        {
            var size = BitConverter.ToInt32(bytes, 0);
            var sizeBytes = BitConverter.GetBytes(size);
            var allBytes = new ReadOnlyMemory<byte>(sizeBytes.Concat(bytes).ToArray());
            await WriteAsync(allBytes);
        }

        /// <summary>
        /// dispose the stream
        /// </summary>
        public void Dispose()
        {
            PipeLineStream.Dispose();
        }
    }
}
