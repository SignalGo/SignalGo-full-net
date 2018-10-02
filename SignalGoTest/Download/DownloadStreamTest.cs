using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2.Models;
using SignalGoTest2Services.StreamServices;
using System.Threading.Tasks;

namespace SignalGoTest.Download
{
    [TestClass]
    public class DownloadStreamTest
    {
        [TestMethod]
        public void TestDownload()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
            SignalGo.Shared.Models.StreamInfo<string> result = service.DownloadImage("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = result.Stream.ReadAsync(bytes, 1024).GetAwaiter().GetResult();
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [TestMethod]
        public async Task TestDownloadAsync()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
            SignalGo.Shared.Models.StreamInfo<string> result = await service.DownloadImageAsync("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = await result.Stream.ReadAsync(bytes, 1024);
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);

        }
    }
}
