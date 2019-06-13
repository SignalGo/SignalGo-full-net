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
        Action FlushAction { get; set; }
        /// <summary>
        /// flush data of stream
        /// </summary>
        Func<Task> FlushAsync { get; set; }
        
        /// <summary>
        /// read data from stream
        /// </summary>
        /// <returns>readed count</returns>
        ReadFunction Read { get; set; }
        /// <summary>
        /// read data from stream by async
        /// </summary>
        /// <returns>readed count</returns>
        ReadAsyncFunction ReadAsync { get; set; }

        /// <summary>
        /// write data to stream
        /// </summary>
        WriteAction Write { get; set; }

        /// <summary>
        /// write data to stream async
        /// </summary>
        WriteAsyncAction WriteAsync { get; set; }

        /// <summary>
        /// read one byte from stream async
        /// </summary>
        /// <returns>byte readed</returns>
        Func<Task<byte>> ReadOneByteAsync { get; set; }
        /// <summary>
        /// read one byte from stream
        /// </summary>
        /// <returns>byte readed</returns>
        Func<byte> ReadOneByte { get; set; }
        /// <summary>
        /// receive data timeouts
        /// </summary>
        int ReceiveTimeout { get; set; }
        /// <summary>
        /// send data timeouts
        /// </summary>
        int SendTimeout { get; set; }

        #region signalgo protocol block system

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
        /// read size of block every blocks has size,this method return size of block to read
        /// </summary>
        /// <returns>array of block size is int32</returns>
        byte[] ReadBlockSize();
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
        /// read size of block every blocks has size,this method return size of block to read
        /// </summary>
        /// <returns>array of block size is int32</returns>
        Task<byte[]> ReadBlockSizeAsync();
        #endregion

        #region pipline
      
        /// <summary>
        /// read new line from stream
        /// </summary>
        /// <returns>line</returns>
        Func<string> ReadLine { get; set; }

        /// <summary>
        /// read new line from stream async
        /// </summary>
        /// <returns>line</returns>
        Func<Task<string>>  ReadLineAsync { get; set; }

        #endregion
    }
}
