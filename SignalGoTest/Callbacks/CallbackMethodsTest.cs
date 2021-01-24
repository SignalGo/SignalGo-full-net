using SignalGoTest2Services.ServerServices;
using System.Threading.Tasks;
using Xunit;

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
    public class CallbackMethodsTest
    {

        [Fact]
        public async Task TestCallbacksAsyncs()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            AuthenticationService service = client.RegisterServerService<AuthenticationService>(client);
            SignalGoTest2.Models.MessageContract result = await service.TestCallbacksAsyncAsync();
            Assert.True(result.IsSuccess);
            SignalGoTest2.Models.MessageContract result2 = await service.TestCallbacksSyncAsync();
            Assert.True(result2.IsSuccess);
        }

        [Fact]
        public void TestCallbacks()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            AuthenticationService service = client.RegisterServerService<AuthenticationService>(client);
            SignalGoTest2.Models.MessageContract result = service.TestCallbacksAsync();
            Assert.True(result.IsSuccess);
            SignalGoTest2.Models.MessageContract result2 = service.TestCallbacksSync();
            Assert.True(result2.IsSuccess);
        }

        [Fact]
        public void TestWebSocketCallbacks()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient(true);
            AuthenticationService service = client.RegisterServerService<AuthenticationService>(client);
            SignalGoTest2.Models.MessageContract result = service.TestCallbacksAsync();
            Assert.True(result.IsSuccess);
            SignalGoTest2.Models.MessageContract result2 = service.TestCallbacksSync();
            Assert.True(result2.IsSuccess);
        }
    }
}
