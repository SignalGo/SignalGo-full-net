using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2Services.ServerServices;
using System.Threading.Tasks;

namespace SignalGoTest.Callbacks
{
    public class TestClientServiceModel : SignalGoTest2Services.ClientServices.ITestClientServiceModel
    {
        public string HelloWorld(string yourName)
        {
            return "Hello";
        }

        public string HelloWorld2(string yourName)
        {
            return "Hello";
        }

        public string TestMethod(string param1, string param2)
        {
            return param1 + " " + param2;
        }

        public string TestMethod2(string param1, string param2)
        {
            return param1 + " " + param2;
        }
    }
    /// <summary>
    /// Summary description for CallbackMethodsTest
    /// </summary>
    [TestClass]
    public class CallbackMethodsTest
    {

        [TestMethod]
        public async Task TestCallbacksAsyncs()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            AuthenticationService service = client.RegisterServerService<AuthenticationService>(client);
            SignalGoTest2.Models.MessageContract result = await service.TestCallbacksAsyncAsync();
            Assert.IsTrue(result.IsSuccess);
            SignalGoTest2.Models.MessageContract result2 = await service.TestCallbacksSyncAsync();
            Assert.IsTrue(result2.IsSuccess);
        }

        [TestMethod]
        public void TestCallbacks()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            AuthenticationService service = client.RegisterServerService<AuthenticationService>(client);
            SignalGoTest2.Models.MessageContract result = service.TestCallbacksAsync();
            Assert.IsTrue(result.IsSuccess);
            SignalGoTest2.Models.MessageContract result2 = service.TestCallbacksSync();
            Assert.IsTrue(result2.IsSuccess);
        }

        [TestMethod]
        public void TestWebSocketCallbacks()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient(true);
            AuthenticationService service = client.RegisterServerService<AuthenticationService>(client);
            SignalGoTest2.Models.MessageContract result = service.TestCallbacksAsync();
            Assert.IsTrue(result.IsSuccess);
            SignalGoTest2.Models.MessageContract result2 = service.TestCallbacksSync();
            Assert.IsTrue(result2.IsSuccess);
        }
    }
}
