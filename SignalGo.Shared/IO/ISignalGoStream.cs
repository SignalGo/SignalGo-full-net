using System.IO;

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
