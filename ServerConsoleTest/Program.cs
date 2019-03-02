using Newtonsoft.Json;
using SignalGo.Server.Models;
using SignalGo.Server.ServiceManager;
using SignalGo.Shared.DataTypes;
using System;
using System.Threading.Tasks;

namespace ServerConsoleTest
{



    [ServiceContract("HealthFamilyClientService", ServiceType.ClientService)]
    public interface ITestClientService
    {
        Task<int> CallMe(string data);
        string ReceivedMessage(string name, string family);
    }

    public class ClientService : ITestClientService
    {
        public Task<int> CallMe(string data)
        {

            throw new Exception();
            return Task.Run(() =>
            {
                return 16;
            });
        }

        public string ReceivedMessage(string name, string family)
        {
            return "ok";
        }
    }

    [ServiceContract("HealthFamilyService", ServiceType.HttpService)]
    [ServiceContract("HealthFamilyService", ServiceType.ServerService)]
    public interface ITestManager
    {
        string HelloWorld(string userName);
        Task<string> HelloWorldAsync(string userName);
        string Test();
        int Sum(int x, int y);
        Task<int> SumAsync(int x, int y);
    }

    public partial class TestService : ITestManager
    {
        public string HelloWorld(string name)
        {
            ITestClientService client = OperationContext.Current.GetClientService<ITestClientService>();
            string result = client.ReceivedMessage("ali", "yousefi");
            Console.WriteLine($"result of ReceivedMessage is {result}");
            //OperationContext<UserInfo>.CurrentSetting = new UserInfo() { Name = userName };
            return $"Hello {name}";
        }

        public Task<string> HelloWorldAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public int Sum(int x, int y)
        {
            return x + y;
        }

        public Task<int> SumAsync(int x, int y)
        {
            throw new NotImplementedException();
        }

        public string Test()
        {
            return OperationContext<UserInfo>.CurrentSetting.Name;
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

    public class UserInfo
    {
        public string Name { get; set; }

        [HttpKey(ResponseHeaderName = "Set-Cookie", RequestHeaderName = "Cookie", Perfix = "; path=/", KeyName = "_session", HeaderValueSeparate = ";", HeaderKeyValueSeparate = "=")]
        public string Session { get; set; }

        [HttpKey(KeyType = HttpKeyType.ExpireField)]
        public DateTime ExpireDateTime { get; set; }
    }

    public class TestAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class AttribClassTest
    {
        [TestAttribute]
        public int Age { get; set; }
    }

    public class SimpleObject
    {
        public string Text { get; set; }
    }

    public class SimpleResultObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public SimpleObject Data { get; set; }
    }

    [ServiceContract("TestService", ServiceType.HttpService, InstanceType = InstanceType.SingleInstance)]
    public class FullHttpSupportService
    {
        public SimpleResultObject TestMethod(string name, int age, SimpleObject data)
        {
            return new SimpleResultObject() { Age = age, Name = name, Data = data };
        }

    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                ServerProvider serverProvider = new ServerProvider();
                serverProvider.RegisterServerService<FullHttpSupportService>();
                serverProvider.Start("http://localhost:8080/TestService/any");
                
                Console.WriteLine("seerver started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }

        public static async void ConnectNewClient()
        {
            SignalGo.Client.ClientProvider clientProvider = new SignalGo.Client.ClientProvider();
            clientProvider.Connect("http://localhost:3284/TestServices/SignalGo");
            ITestManager service = clientProvider.RegisterServerServiceInterfaceWrapper<ITestManager>();
            string result = await service.HelloWorldAsync("ali123");
        }

        //public static void PiplineTest()
        //{
        //    TcpListener tcpListener = new TcpListener(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 4545);

        //    tcpListener.Start();

        //    var socket = tcpListener.AcceptSocket();
        //}


        private static async void ClientAutoReconnectTest()
        {
            SignalGo.Client.ClientProvider clientProvider = new SignalGo.Client.ClientProvider();
            clientProvider.ProviderSetting.PriorityFunctionDelayTime = 0;
            ITestManager service = clientProvider.RegisterServerServiceInterfaceWrapper<ITestManager>();
            clientProvider.RegisterClientService<ClientService>();
            clientProvider.ConnectAsyncAutoReconnect("http://localhost:9752/SignalGoTestService", async (isConnected) =>
            {
                try
                {
                    Console.WriteLine("connection changed: " + isConnected);
                    if (isConnected)
                    {
                        Console.WriteLine("sum async calling");
                        for (int i = 0; i < 100; i++)
                        {
                            int sumResult = await service.SumAsync(10, 5);
                            await Task.Delay(1000);
                            Console.WriteLine("sum async called: " + sumResult);
                        }
                    }
                }
                catch
                {

                }
            });

            clientProvider.AddPriorityAsyncFunction(async () =>
            {
                try
                {
                    Console.WriteLine("HelloWorldAsync Calling");
                    string result = await service.HelloWorldAsync("ali123");
                    Console.WriteLine("HelloWorldAsync Success " + result);
                    if (result == $"Hello ali123")
                        return SignalGo.Client.PriorityAction.NoPlan;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return SignalGo.Client.PriorityAction.TryAgain;
            });
            
        }
    }
}
