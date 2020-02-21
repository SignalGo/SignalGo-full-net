using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.IO.Compressions
{
    /// <summary>
    /// trhere is no compress mode
    /// </summary>
    public class NoCompression : ICompression
    {
        /// <summary>
        /// compress data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Compress(ref byte[] input)
        {
            return input;
        }

        /// <summary>
        /// decompress data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Decompress(ref byte[] input)
        {
            return input;
        }
    }
}
