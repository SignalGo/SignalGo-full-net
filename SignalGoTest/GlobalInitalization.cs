using SignalGo.Client;
using SignalGo.Server.ServiceManager;
using SignalGoTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest
{
    public static class GlobalInitalization
    {
        static ServerProvider server;
        static ClientProvider client;

        public static void Initialize()
        {
            if (server == null)
            {
                server = new ServerProvider();
                server.RegisterStreamService(typeof(TestServerStreamModel));
                server.Start("http://localhost:1132/SignalGoTestService");
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
