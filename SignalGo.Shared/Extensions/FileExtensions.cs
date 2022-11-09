using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileExtensions
    {
        //idea from: https://stackoverflow.com/a/22733709/18026723
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        /// <summary>
        /// Converts file size in bytes to specified type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double ToSize(this Int64 value, SizeUnits unit)
        {
            return (value / (double)Math.Pow(1024, (Int64)unit));
        }
    }
}
