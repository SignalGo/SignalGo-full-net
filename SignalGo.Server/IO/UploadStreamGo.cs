using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SignalGo.Server.IO
{
    //internal class UploadStreamGo : Stream
    //{
    //    NetworkStream CurrentStream { get; set; }

    //    public UploadStreamGo(NetworkStream currentStream)
    //    {
    //        CurrentStream = currentStream;
    //    }

    //    long _Length;

    //    long _Position;
    //    public override bool CanRead => CurrentStream.CanRead;

    //    public override bool CanSeek => CurrentStream.CanSeek;

    //    public override bool CanWrite => CurrentStream.CanWrite;

    //    public override long Length => _Length;

    //    public override long Position { get => _Position; set => _Position = value; }

    //    public override void Flush()
    //    {
    //        CurrentStream.Flush();
    //    }

    //    public void SetLengthOfBase(long length)
    //    {
    //        _Length = length;
    //    }

    //    public bool IsFinished
    //    {
    //        get
    //        {
    //            return _Length == _Position;
    //        }
    //    }

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        int readCount = CurrentStream.Read(buffer, offset, count);
    //        Position += readCount;
    //        return readCount;
    //    }

    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        return CurrentStream.Seek(offset, origin);
    //    }

    //    public override void SetLength(long value)
    //    {
    //        CurrentStream.SetLength(value);
    //    }

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        CurrentStream.Write(buffer, offset, count);
    //    }
    //}
}
