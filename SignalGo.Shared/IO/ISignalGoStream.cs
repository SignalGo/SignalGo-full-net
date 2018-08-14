using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public interface ISignalGoStream
    {
        void WriteToStream(Stream stream, byte[] data);
        byte[] EncodeMessageToSend(byte[] bytesRaw);
        byte[] ReadBlockToEnd(Stream stream, CompressMode compress, uint maximum);
        void WriteBlockToStream(Stream stream, byte[] data);
        byte ReadOneByte(Stream stream, CompressMode compress, uint maximum);
    }
}
