using System;
using System.IO;
using System.Threading.Tasks;

namespace SignalGo.Shared.IO
{
    /// <summary>
    /// normal read and write stream
    /// </summary>
    public class NormalStream : IStream
    {
        PipeLineStream PipeLineStream { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeLineStream"></param>
        public NormalStream(PipeLineStream pipeLineStream)
        {
            PipeLineStream = pipeLineStream;
            FlushAction = PipeLineStream.FlushAction;
            FlushAsync = PipeLineStream.FlushAsyncAction;
            Read = PipeLineStream.ReadAction;
            ReadAsync = PipeLineStream.ReadAsyncAction;
            Write = PipeLineStream.WriteAction;
            WriteAsync = PipeLineStream.WriteAsyncAction;
            ReadOneByte = PipeLineStream.ReadOneByte;
            ReadOneByteAsync = PipeLineStream.ReadOneByteAsync;
            ReadLine = PipeLineStream.ReadLine;
            ReadLineAsync = PipeLineStream.ReadLineAsync;
        }
        /// <summary>
        /// receive data timeouts
        /// </summary>
        public int ReceiveTimeout { get; set; } = -1;
        /// <summary>
        /// send data timeouts
        /// </summary>
        public int SendTimeout { get; set; } = -1;
        /// <summary>
        /// flush data of stream
        /// </summary>
        public Action FlushAction { get; set; }
        /// <summary>
        /// flush data of stream
        /// </summary>
        public Func<Task> FlushAsync { get; set; }

        /// <summary>
        /// read data from stream
        /// </summary>
        /// <returns>readed count</returns>
        public ReadFunction Read { get; set; }
        /// <summary>
        /// read data from stream by async
        /// </summary>
        /// <returns>readed count</returns>
        public ReadAsyncFunction ReadAsync { get; set; }

        /// <summary>
        /// write data to stream
        /// </summary>
        public WriteAction Write { get; set; }

        /// <summary>
        /// write data to stream async
        /// </summary>
        public WriteAsyncAction WriteAsync { get; set; }

        /// <summary>
        /// read one byte from stream async
        /// </summary>
        /// <returns>byte readed</returns>
        public Func<Task<byte>> ReadOneByteAsync { get; set; }
        /// <summary>
        /// read one byte from stream
        /// </summary>
        /// <returns>byte readed</returns>
        public Func<byte> ReadOneByte { get; set; }


        /// <summary>
        /// read new line from stream
        /// </summary>
        /// <returns>line</returns>
        public Func<string> ReadLine { get; set; }

        /// <summary>
        /// read new line from stream async
        /// </summary>
        /// <returns>line</returns>
        public Func<Task<string>> ReadLineAsync { get; set; }


        public byte[] ReadBlockToEnd(ref int maximum)
        {
            throw new NotImplementedException();
        }

        public void WriteBlockToStream(ref byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBlockSize()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadBlockToEndAsync(ref int maximum)
        {
            throw new NotImplementedException();
        }

        public Task WriteBlockToStreamAsync(ref byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadBlockSizeAsync()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// dispose the stream
        /// </summary>
        public void Dispose()
        {
            PipeLineStream.Dispose();
        }
    }
}
