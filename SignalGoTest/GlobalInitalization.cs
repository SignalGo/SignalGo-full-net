using SignalGo.Client;
using SignalGo.Server.ServiceManager;
using SignalGoTest.Models;
using System.Collections.Generic;

namespace SignalGoTest
{
    public static class GlobalInitalization
    {
        private static ServerProvider server;
        private static ClientProvider client;

        public static void Initialize()
        {
            if (server == null)
            {
                server = new SignalGo.Server.ServiceManager.ServerProvider();
                server.RegisterServerService<Models.TestServerStreamModel>();
                server.RegisterServerService<Models.TestServerModel>();
                server.Start("http://localhost:1132/SignalGoTestService");
                server.OnConnectedClientAction = (client) =>
                {

                };
                server.OnDisconnectedClientAction = (client) =>
                {

                };
                server.ValidationResultHandlingFunction = (errors, service, method) =>
                {
                    List<Models.ValidationRule> result = new List<Models.ValidationRule>();
                    foreach (BaseValidationRuleAttribute item in errors)
                    {
                        result.Add(new Models.ValidationRule() { Message = item.Message, Name = item.PropertyInfo?.Name });

                    }
                    return new MessageContract<ArticleInfo>() { IsSuccess = false, Errors = result };
                };
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
            client = provider;
            //connect to your server must have full address that your server is listen
            provider.Connect("http://localhost:1132/SignalGoTestService");
            return provider;
        }
    }
}
