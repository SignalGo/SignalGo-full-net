using SignalGo.Client;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsoleTest
{
    [ServiceContract("HealthFamilyService", ServiceType.HttpService)]
    public interface ITestManager
    {
        bool HelloWorld(string userName, string password);
        string HelloWorld2(string userName, string password);
    }

    public partial class TestService : ITestManager
    {
        public bool HelloWorld(string userName, string password)
        {
            return true;
        }

        public string HelloWorld2(string userName, string password)
        {
            return "hello deafult : " + password;
        }
    }

    [ServiceContract("HttpService", ServiceType.HttpService)]
    public partial class HttpService
    {
        public bool HelloWorld(string userName, string password)
        {
            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServerProvider serverProvider = new ServerProvider();
                serverProvider.RegisterServerService(typeof(TestService));
                serverProvider.Start("http://localhost:3284/TestServices/SignalGo");

                //ClientProvider clientProvider = new ClientProvider();
                //clientProvider.Connect("http://localhost:3284/TestServices/SignalGo");
                //var service = clientProvider.RegisterServerServiceInterfaceWrapper<ITestManager>();
                //var result = service.HelloWorld2("userName","passee");
                Console.WriteLine("seerver started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
    }
}
