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
