using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsoleTest
{
    [ServiceContract("HealthFamilyService", ServiceType.ServerService)]
    public interface ITestManager
    {
        bool HelloWorld(string userName, string password);
    }

    public partial class TestService : ITestManager
    {
        public bool HelloWorld(string userName, string password)
        {
            return true;
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
                ServerProvider provider = new ServerProvider();
                provider.Start("http://localhost:3284/TestServices/SignalGo");
                Console.WriteLine("server started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
    }
}
