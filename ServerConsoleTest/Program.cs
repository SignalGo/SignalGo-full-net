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

                //SignalGoBlazorHttpClient signalGoBlazorHttpClient = new SignalGoBlazorHttpClient();
                //SignalGo.Http.HttpClientResponse result = signalGoBlazorHttpClient.PostAsync("http://dev.atitec.ir:1747/Authentication/Login", new ParameterInfo[] {
                //    new ParameterInfo()
                //    {
                //         Name = "userName",
                //         Value="98test"
                //    } ,
                //    new ParameterInfo()
                //    {
                //         Name = "password",
                //         Value="d5e4aa21-6fe2-4fbc-9264-a14e14698564868385e8-2922-41c0-a4cf-29fb65203c28"
                //    }
                //}).GetAwaiter().GetResult();
                //HttpClient tesClient = new HttpClient();
                //tesClient.Post("http://panel.menno.ir/Authentication/Login", new SignalGo.Shared.Models.ParameterInfo[] {
                //    new SignalGo.Shared.Models.ParameterInfo(){ Name = "username" , Value ="ali"},
                //    new SignalGo.Shared.Models.ParameterInfo(){ Name ="password",Value = "12345" }
                //});
                //PipeNetworkStream pipeNetworkStream = new PipeNetworkStream(null);
                //byte[] result = pipeNetworkStream.Read(100, out int readCount);
                //ServerProvider serverProvider = new ServerProvider();
                //serverProvider.RegisterServerService(typeof(TestService));
                //serverProvider.RegisterClientService(typeof(ITestClientService));
                //serverProvider.Start("http://localhost:9752/SignalGoTestService");
                //Thread.Sleep(2000);
                //Thread thread2 = new Thread(() =>
                //{
                //    for (int i = 0; i < 10; i++)
                //    {
                //        ConnectNewClient();
                //    }
                //});
                //thread2.Start();

                //ClientProvider clientProvider2 = new ClientProvider();
                //clientProvider2.Connect("http://localhost:3284/TestServices/SignalGo");
                //var service2 = clientProvider2.RegisterServerServiceInterfaceWrapper<ITestManager>();
                //clientProvider2.RegisterClientService<ClientService>();
                //var result3 = service2.HelloWorld("reza123", "passee");
                //var result4 = service2.Test();
                //result2 = service.Test();
                //ClientAutoReconnectTest();
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

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    clientProvider.TestDisConnect();
                }
            });
        }
    }
}
