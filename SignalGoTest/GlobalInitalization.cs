using SignalGo.Client;
using SignalGo.Server.ServiceManager;
using SignalGoTest.Callbacks;
using SignalGoTest.Models;
using System.Collections.Generic;

namespace SignalGoTest
{
    public static class GlobalInitalization
    {
        private static ServerProvider server;

        static GlobalInitalization()
        {
            server = new SignalGo.Server.ServiceManager.ServerProvider();
            server.RegisterServerService<Models.TestServerStreamModel>();
            server.RegisterServerService<Models.TestServerModel>();
            server.RegisterServerService<Models.AuthenticationService>();
            server.RegisterClientService<Models.ITestClientServiceModel>();
            server.Start("http://localhost:1132/SignalGoTestService");
            server.ErrorHandlingFunction = (ex, type, method, parameters, jsonParameter, client) =>
            {
                return new MessageContract() { IsSuccess = false, Message = ex.ToString() };
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
        }

        public static ClientProvider InitializeAndConnecteClient(bool isWebSocket = false)
        {
            ClientProvider provider = new ClientProvider();
            if (isWebSocket)
                provider.ProtocolType = SignalGo.Client.ClientManager.ClientProtocolType.WebSocket;
            //connect to your server must have full address that your server is listen
            provider.Connect("http://localhost:1132/SignalGoTestService");
            provider.RegisterClientService<TestClientServiceModel>();

            return provider;
        }
    }
}
