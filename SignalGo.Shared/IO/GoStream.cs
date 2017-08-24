using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SignalGo.Shared.IO
{
    //public class GoStream
    //{
    //    public GoStream(NetworkStream stream, bool isReadOnly, bool isWebSocket)
    //    {
    //        IsReadOnly = isReadOnly;
    //        Stream = stream;
    //    }

    //    NetworkStream Stream { get; set; }
    //    bool IsReadOnly { get; set; }
    //    bool IsWebSocket { get; set; }

    //    public bool CanRead
    //    {
    //        get
    //        {
    //            return IsReadOnly;
    //        }
    //    }

    //    public bool CanWrite
    //    {
    //        get
    //        {
    //            return !IsReadOnly;
    //        }
    //    }

    //    public long Position { get; private set; }

    //    public int Read(byte[] buffer, int offset, int count)
    //    {
    //        if (!CanRead)
    //            throw new Exception("stream is not readable!");
    //        var readCount = Stream.Read(buffer, offset, count);
    //        Position += readCount;
    //        return readCount;
    //    }

    //    public int ReadByte()
    //    {
    //        if (!CanRead)
    //            throw new Exception("stream is not readable!");
    //        return Stream.ReadByte();
    //    }

    //    public void Write(byte[] buffer, int offset, int count)
    //    {
    //        if (!CanWrite)
    //            throw new Exception("stream is not writable!");
    //        Stream.Write(buffer, offset, count);
    //    }

    //    public void WriteByte(byte value)
    //    {
    //        if (!CanWrite)
    //            throw new Exception("stream is not writable!");
    //        Stream.WriteByte(value);
    //    }
    //}
}
