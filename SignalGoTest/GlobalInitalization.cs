using SignalGo.Client;
using SignalGoTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalGo.Shared;
using System.Threading;

namespace SignalGoTest
{
    public static class GlobalInitalization
    {
        static SignalGo.Server.ServiceManager.ServerProvider server;
        //static ClientProvider client;

        public static void Initialize()
        {
            if (server == null)
            {
                server = new SignalGo.Server.ServiceManager.ServerProvider();
                server.RegisterStreamService(typeof(TestServerStreamModel));
                server.RegisterServerService<TestServerModel>();
                server.RegisterClientService<TestServerModel>();
                server.Start("http://localhost:1132/SignalGoTestService");
                server.OnConnectedClientAction = (client) =>
                {

                };
                server.OnDisconnectedClientAction = (client) =>
                {

                };
                server.InternalSetting = new SignalGo.Server.Settings.InternalSetting() { IsEnabledDataExchanger = true };
                ////your client connector that will be connect to your server
                //ClientProvider provider = new ClientProvider();
                ////connect to your server must have full address that your server is listen
                //provider.Connect("http://localhost:1132/SignalGoTestService");
                //var service = provider.RegisterClientServiceInterfaceWrapper<ITestClientServerModel>();

                //try
                //{
                //    var result = service.HelloWorld("ali");
                //    //var result1 = await service.MUL(10, 20);
                //    //var result3 = await service.WhoAmI();
                //    //var result40 = service.Tagh(10, 3);
                //    ////var result41 = service.Tagha(10, 3);
                //    //var result4 = await service.TaghAsync(10, 3);
                //    //var result5 = await service.LongValue();
                //    //var result6 = await service.TimeS(100000000);
                //}
                //catch (Exception ex)
                //{

                //}
                ////register your service interfacce for client
                ////var testServerModel = provider.RegisterClientServiceDynamic<ITestServerModel>();
                ////call server method and return value from your server to client
                ////var result = testServerModel.HelloWorld("ali");
                //provider.Dispose();
                //Thread.Sleep(10000);
                //print your result to console
                //Console.WriteLine(result.Item1);
            }
            //client = new ClientProvider();
            //client.Connect("http://localhost:1132/SignalGoTestService");
        }

        public static ClientProvider InitializeAndConnecteClient()
        {
            ClientProvider provider = new ClientProvider();
            //connect to your server must have full address that your server is listen
            provider.Connect("http://localhost:1132/SignalGoTestService");
            return provider;
        }

        //public static ITestServerStreamModel GetStreamService()
        //{
        //    return client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
        //}
    }
}
