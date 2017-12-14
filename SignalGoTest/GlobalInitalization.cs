using SignalGo.Client;
using SignalGoTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalGo.Shared;

namespace SignalGoTest
{
    public static class GlobalInitalization
    {
        static SignalGo.Server.ServiceManager.ServerProvider server;
        static ClientProvider client;

        public static void Initialize()
        {
            if (server == null)
            {
                server = new SignalGo.Server.ServiceManager.ServerProvider();
                server.RegisterStreamService(typeof(TestServerStreamModel));
                server.Start("http://localhost:1132/SignalGoTestService");
                
                //your client connector that will be connect to your server
                ClientProvider provider = new ClientProvider();
                //connect to your server must have full address that your server is listen
                provider.Connect("http://localhost:1132/SignalGoTestService");
                //register your service interfacce for client
                var testServerModel = provider.RegisterClientServiceDynamic<ITestServerModel>();
                //call server method and return value from your server to client
                var result = testServerModel.HelloWorld("ali");
                //print your result to console
                Console.WriteLine(result.Item1);
            }
            client = new ClientProvider();
            client.Connect("http://localhost:1132/SignalGoTestService");
        }

        public static ITestServerStreamModel GetStreamService()
        {
            return client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
        }
    }
}
