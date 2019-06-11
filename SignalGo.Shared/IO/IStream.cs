// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net


using System;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    /// <summary>
    /// stream interface of signalgo data provider
    /// </summary>
    public interface IStream : IDisposable
    {
        /// <summary>
        /// flush data of stream
        /// </summary>
        void Flush();
        /// <summary>
        /// read data from stream
        /// </summary>
        /// <param name="buffer">buffer to read</param>
        /// <param name="count">count to read</param>
        /// <returns>readed count</returns>
        int Read(ref byte[] buffer, ref int count);
        /// <summary>
        /// write data to stream
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        /// <param name="count">count to write stream</param>
        void Write(ref byte[] buffer, ref int count);
        /// <summary>
        /// receive data timeouts
        /// </summary>
        int ReceiveTimeout { get; set; }
        /// <summary>
        /// send data timeouts
        /// </summary>
        int SendTimeout { get; set; }
        /// <summary>
        /// flush async stream
        /// </summary>
        /// <returns></returns>
        Task FlushAsync();
        /// <summary>
        /// read data from stream by async
        /// </summary>
        /// <param name="buffer">buffer to read</param>
        /// <param name="count">count to read</param>
        /// <returns>readed count</returns>
        Task<int> ReadAsync(ref byte[] buffer, ref int count);
        /// <summary>
        /// write data to stream async
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        /// <param name="count">count to write stream</param>
        Task WriteAsync(ref byte[] buffer, ref int count);

        #region signalgo protocol block system
        /// <summary>
        /// write byte array to stream
        /// </summary>
        /// <param name="bytes">byte array to write</param>
        void Write(ref byte[] bytes);
        /// <summary>
        /// read block of signalgo packet to end of packet
        /// </summary>
        /// <param name="maximum">maximum size of read</param>
        /// <returns></returns>
        byte[] ReadBlockToEnd(ref int maximum);
        /// <summary>
        /// write block of signalgo packet bytes array to stream
        /// </summary>
        /// <param name="bytes">bytes to write</param>
        void WriteBlockToStream(ref byte[] bytes);
        /// <summary>
        /// read one byte from stream
        /// </summary>
        /// <returns>byte readed</returns>
        byte ReadOneByte();
        /// <summary>
        /// read size of block every blocks has size,this method return size of block to read
        /// </summary>
        /// <returns>array of block size is int32</returns>
        byte[] ReadBlockSize();
        /// <summary>
        /// write byte array to stream async
        /// </summary>
        /// <param name="bytes">byte array to write</param>
        Task WriteAsync(ref byte[] bytes);
        /// <summary>
        /// read block of signalgo packet to end of packet async
        /// </summary>
        /// <param name="maximum">maximum size of read</param>
        /// <returns></returns>
        Task<byte[]> ReadBlockToEndAsync(ref int maximum);
        /// <summary>
        /// write block of signalgo packet bytes array to stream async
        /// </summary>
        /// <param name="bytes">bytes to write</param>
        Task WriteBlockToStreamAsync(ref byte[] bytes);
        /// <summary>
        /// read one byte from stream async
        /// </summary>
        /// <returns>byte readed</returns>
        Task<byte> ReadOneByteAsync();
        /// <summary>
        /// read size of block every blocks has size,this method return size of block to read
        /// </summary>
        /// <returns>array of block size is int32</returns>
        Task<byte[]> ReadBlockSizeAsync();
        #endregion

        #region pipline
        /// <summary>
        /// read bytes from existing byte for example for newline
        /// </summary>
        /// <param name="exitBytes">existing bytes</param>
        /// <returns>readed bytes</returns>
        byte[] Read(ref byte[] exitBytes);
        /// <summary>
        /// read new line from stream
        /// </summary>
        /// <returns>line</returns>
        string ReadLine();
        /// <summary>
        /// read new line from stream with endofline chars
        /// </summary>
        /// <param name="endOfLine">end of line chars</param>
        /// <returns>new line</returns>
        string ReadLine(ref string endOfLine);

        /// <summary>
        /// read bytes from existing byte for example for newline async
        /// </summary>
        /// <param name="exitBytes">existing bytes</param>
        /// <returns>readed bytes</returns>
        Task<byte[]> ReadAsync(ref byte[] exitBytes);

        /// <summary>
        /// read new line from stream async
        /// </summary>
        /// <returns>line</returns>
        Task<string> ReadLineAsync();
        /// <summary>
        /// read new line from stream with endofline chars async
        /// </summary>
        /// <param name="endOfLine">end of line chars</param>
        /// <returns>new line</returns>
        Task<string> ReadLineAsync(ref string endOfLine);

        #endregion
    }
}
