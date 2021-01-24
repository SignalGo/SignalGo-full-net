using SignalGoTest2Services.Interfaces;
using System.Threading.Tasks;
using Xunit;

namespace SignalGoTest.AsyncAwaitCalls
{
    public class AsyncAwaitTest
    {
        [Fact]
        public async Task TestAsyncs()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            string result = service.ServerAsyncMethod("hello");
            Assert.True(result == "hello guys");
            string result2 = service.ServerAsyncMethod("hello2");
            Assert.True(result2 == "not found");

            result = await service.ServerAsyncMethodAsync("hello");
            Assert.True(result == "hello guys");
            result2 = await service.ServerAsyncMethodAsync("hello2");
            Assert.True(result2 == "not found");
        }

    }
}
