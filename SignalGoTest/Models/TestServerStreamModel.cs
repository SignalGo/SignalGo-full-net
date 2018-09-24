using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class TestServerStreamModel : ITestServerStreamModel
    {
        public StreamInfo<string> DownloadImage(string name, TestStreamModel testStreamModel)
        {
            return new StreamInfo<string>() { Data = "hello return", Length = 4, Stream = new MemoryStream(new byte[] { 2, 5, 8, 9 }) };
        }

        public string UploadImage(string name, StreamInfo streamInfo, TestStreamModel testStreamModel)
        {
            return null;
        }
    }
}
