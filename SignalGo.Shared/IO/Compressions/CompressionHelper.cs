using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.IO.Compressions
{
    public static class CompressionHelper
    {
        public static ICompression GetCompression(CompressMode compressMode, Func<ICompression> getCustomCompression)
        {
            if (compressMode == CompressMode.None)
                return new NoCompression();
            if (getCustomCompression != null)
                return getCustomCompression();
            throw new NotSupportedException();
        }
    }
}
