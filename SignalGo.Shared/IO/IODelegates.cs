// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    /// <summary>
    /// function to read bytes from stream
    /// </summary>
    /// <param name="bytes">bytes space to read, after read stream will fill your bytes array here</param>
    /// <param name="offset">offset</param>
    /// <param name="count">count of read from stream</param>
    /// <returns>count of readed bytes from stream</returns>
    public delegate Task<int> ReadAsyncFunction(byte[] bytes, int offset, int count);
    /// <summary>
    /// function to read bytes from stream
    /// </summary>
    /// <param name="bytes">bytes space to read, after read stream will fill your bytes array here</param>
    /// <param name="offset">offset</param>
    /// <param name="count">count of read from stream</param>
    /// <returns>count of readed bytes from stream</returns>
    public delegate int ReadFunction(byte[] bytes, int offset, int count);
    /// <summary>
    /// read a bytes array to end with a count you want to read
    /// </summary>
    /// <param name="count">count to read data</param>
    /// <returns>byte array readed</returns>
    public delegate byte[] ReadToEndFunction(int count);
    /// <summary>
    /// read a bytes array to end with a count you want to read
    /// </summary>
    /// <param name="count">count to read data</param>
    /// <returns>byte array readed</returns>
    public delegate Task<byte[]> ReadToEndAsyncFunction(int count);
    /// <summary>
    /// write data to stream
    /// </summary>
    /// <param name="bytes">bytes of data</param>
    /// <param name="offset">offset</param>
    /// <param name="count">count of data</param>
    public delegate void WriteAction(byte[] bytes, int offset, int count);
    /// <summary>
    /// write data to stream
    /// </summary>
    /// <param name="bytes">bytes of data</param>
    /// <param name="offset">offset</param>
    /// <param name="count">count of data</param>
    public delegate Task WriteAsyncAction(byte[] bytes, int offset, int count);
}
