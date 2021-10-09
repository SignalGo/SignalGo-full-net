using SignalGo.Shared.Models;
using System.IO;

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
            int lengthWrite = 0;
            while (lengthWrite != streamInfo.Length)
            {
                byte[] bytes = new byte[1024];
                int readCount = streamInfo.Stream.ReadAsync(bytes, bytes.Length).ConfigureAwait(false).GetAwaiter().GetResult();
                if (readCount <= 0)
                    break;
                lengthWrite += readCount;
                //if you have a progress bar in client side this code will send your server position to client and client can position it if you don't have progressbar just pervent this line
                //streamInfo.SetPositionFlushAsync(lengthWrite).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return "success";
        }
    }
}
