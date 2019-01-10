using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.Owin
{
    public class AspNetStream : IStream
    {
        public int ReceiveTimeout { get; set; }

        public int SendTimeout { get; set; }

        public void Dispose()
        {

        }

        public void Flush()
        {

        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int offset, int count)
        {

        }

        public Task WriteAsync(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
