﻿using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    public interface ISignalGoStream
    {
        byte[] EncodeMessageToSend(byte[] bytesRaw);
#if (NET35 || NET40)
        void WriteToStream(PipeNetworkStream stream, byte[] data);
        byte[] ReadBlockToEnd(PipeNetworkStream stream, CompressMode compress, int maximum);
        void WriteBlockToStream(PipeNetworkStream stream, byte[] data);
        byte ReadOneByte(PipeNetworkStream stream);
        byte[] ReadBlockSize(PipeNetworkStream stream, int count);
#else
        Task WriteToStreamAsync(PipeNetworkStream stream, byte[] data);
        Task<byte[]> ReadBlockToEndAsync(PipeNetworkStream stream, CompressMode compress, int maximum);
        Task WriteBlockToStreamAsync(PipeNetworkStream stream, byte[] data);
        Task<byte> ReadOneByteAsync(PipeNetworkStream stream);
        Task<byte[]> ReadBlockSizeAsync(PipeNetworkStream stream, int count);
#endif
        //#if (NET35 || NET40)
        //        byte ReadOneByteAsync(IStream stream);
        //#else
        //        Task<byte> ReadOneByteAsync(IStream stream);
        //#endif
    }
}