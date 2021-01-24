using SignalGoTest2.Models;
using SignalGoTest2Services.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SignalGoTest.Download
{
    public class DownloadStreamTest
    {
        [Fact]
        public void TestDownload()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
            SignalGo.Shared.Models.StreamInfo<string> result = service.DownloadImage("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = result.Stream.ReadAsync(bytes, 1024).GetAwaiter().GetResult();
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [Fact]
        public async Task TestDownloadAsync()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
            SignalGo.Shared.Models.StreamInfo<string> result = await service.DownloadImageAsync("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = await result.Stream.ReadAsync(bytes, 1024);
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [Fact]
        public void TestUpload()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
                string result = service.UploadImage("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.True(result == "success", result);
            }
        }

        [Fact]
        public async Task TestUploadAsync()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("http://localhost:1132");
                string result = await service.UploadImageAsync("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.True(result == "success");
            }
        }




        [Fact]
        public void TestDownloadCross()
        {
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            SignalGo.Shared.Models.StreamInfo<string> result = service.DownloadImage("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = result.Stream.ReadAsync(bytes, 1024).GetAwaiter().GetResult();
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);

        }

        [Fact]
        public async Task TestDownloadCrossAsync()
        {
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            SignalGo.Shared.Models.StreamInfo<string> result = await service.DownloadImageAsync("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = await result.Stream.ReadAsync(bytes, 1024);
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [Fact]
        public void TestUploadCross()
        {
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                string result = service.UploadImage("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.True(result == "success");
            }
        }

        [Fact]
        public async Task TestUploadCrossAsync()
        {
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                string result = await service.UploadImageAsync("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.True(result == "success");
            }
        }
    }
}
