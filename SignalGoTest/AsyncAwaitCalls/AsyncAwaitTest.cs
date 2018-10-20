using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2Services.Interfaces;
using SignalGoTest2Services.ServerServices;
using System.Threading.Tasks;

namespace SignalGoTest.AsyncAwaitCalls
{
    [TestClass]
    public class AsyncAwaitTest
    {
        [TestMethod]
        public async Task TestAsyncs()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            string result = service.ServerAsyncMethod("hello");
            Assert.IsTrue(result == "hello guys");
            string result2 = service.ServerAsyncMethod("hello2");
            Assert.IsTrue(result2 == "not found");

            result = await service.ServerAsyncMethodAsync("hello");
            Assert.IsTrue(result == "hello guys");
            result2 = await service.ServerAsyncMethodAsync("hello2");
            Assert.IsTrue(result2 == "not found");
        }
    }
}
