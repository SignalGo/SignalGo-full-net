using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public interface ISignalGoStream
    {
        void WriteToStream(PipeNetworkStream stream, byte[] data);
        byte[] EncodeMessageToSend(byte[] bytesRaw);
        byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, uint maximum);
        void WriteBlockToStream(PipeNetworkStream stream, byte[] data);
        byte ReadOneByte(PipeNetworkStream stream);
        byte[] ReadBlockSize(PipeNetworkStream stream, ulong count);
//#if (NET35 || NET40)
//        byte ReadOneByteAsync(IStream stream);
//#else
//        Task<byte> ReadOneByteAsync(IStream stream);
//#endif
    }
}
