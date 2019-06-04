using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public interface ISignalGoStream
    {
        void WriteToStream(PipeNetworkStream stream, byte[] data);
        byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum);
        void WriteBlockToStream(PipeNetworkStream stream, byte[] data);
        byte ReadOneByte(PipeNetworkStream stream);
        byte[] ReadBlockSize(PipeNetworkStream stream, int count);
//#if (NET35 || NET40)
//        string ReadLine(PipeNetworkStream stream, string exitCode);
//#endif

#if (!NET35 && !NET40)
        Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data);
        Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, CompressMode compress, int maximum);
        Task WriteBlockToStreamAsync(PipeNetworkStream stream, byte[] data);
        Task<byte> ReadOneByteAsync(PipeNetworkStream stream);
        Task<byte[]> ReadBlockSizeAsync(PipeNetworkStream stream, int count);
        //Task<string> ReadLineAsync(PipeNetworkStream stream, string exitCode); 
#endif
        //#if (NET35 || NET40)
        //        byte ReadOneByteAsync(IStream stream);
        //#else
        //        Task<byte> ReadOneByteAsync(IStream stream);
        //#endif
    }
}
