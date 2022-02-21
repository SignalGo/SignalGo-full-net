using SignalGo.Client;
using SignalGo.Shared.IO.Compressions;
using SignalGoTest2.Models;
using SignalGoTest2Services.Interfaces;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xunit;

namespace SignalGoTest.Compressions
{
    public class CompressionTest
    {
        [Fact]
        public async Task TestCompressionAsync()
        {
            var server = new SignalGo.Server.ServiceManager.ServerProvider();
            server.RegisterServerService<Models.TestServerStreamModel>();
            server.RegisterServerService<Models.TestServerModel>();
            server.RegisterServerService<Models.AuthenticationService>();
            server.RegisterClientService<Models.ITestClientServiceModel>();
            server.Start("http://localhost:1133/SignalGoTestService");
            server.ErrorHandlingFunction = (ex, serviceType, method, client) =>
            {
                return new MessageContract() { IsSuccess = false, Message = ex.ToString() };
            };
            server.CurrentCompressionMode = SignalGo.CompressMode.Custom;
            server.GetCustomCompression = () =>
            {
                return new GZipCompressionTest();
            };
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                ClientProvider clientProvider = new ClientProvider();
                clientProvider.CurrentCompressionMode = SignalGo.CompressMode.Custom;
                clientProvider.GetCustomCompression = server.GetCustomCompression;
                ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("http://localhost:1133", null, clientProvider);
                string result = await service.UploadImageAsync("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.True(result == "success");
            }
        }
    }

    public class GZipCompressionTest : ICompression
    {
        public byte[] Compress(ref byte[] input)
        {
            using (var outStream = new MemoryStream())
            {
                using (var tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var mStream = new MemoryStream(input))
                    mStream.CopyTo(tinyStream);
                return outStream.ToArray();
            }
        }

        public byte[] Decompress(ref byte[] input)
        {
            using (var inStream = new MemoryStream(input))
            using (var bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var bigStreamOut = new MemoryStream())
            {
                bigStream.CopyTo(bigStreamOut);
                return bigStreamOut.ToArray();
            }
        }
    }
}
