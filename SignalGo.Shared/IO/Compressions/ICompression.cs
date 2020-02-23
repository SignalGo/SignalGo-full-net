using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.IO.Compressions
{
    /// <summary>
    /// to compress and decompress your data
    /// </summary>
    public interface ICompression
    {
        /// <summary>
        /// compress data 
        /// </summary>
        /// <param name="input">input bytes to compress</param>
        /// <returns></returns>
        byte[] Compress(ref byte[] input);
        /// <summary>
        /// decompress data
        /// </summary>
        /// <param name="input">data to decompress</param>
        /// <returns></returns>
        byte[] Decompress(ref byte[] input);
    }
}
