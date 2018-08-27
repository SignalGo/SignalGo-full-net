using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public interface ISignalGoStream
    {
        void WriteToStream(Stream stream, byte[] data);
        byte[] EncodeMessageToSend(byte[] bytesRaw);
        byte[] ReadBlockToEnd(Stream stream, CompressMode compress, uint maximum);
        void WriteBlockToStream(Stream stream, byte[] data);
        byte ReadOneByte(Stream stream);
        byte[] ReadBlockSize(Stream stream, ulong count);
#if (NET35 || NET40)
        byte ReadOneByteAsync(Stream stream);
#else
        Task<byte> ReadOneByteAsync(Stream stream);
#endif
    }
}
