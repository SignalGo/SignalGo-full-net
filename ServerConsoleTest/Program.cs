using SignalGo.Server.Models;
using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerConsoleTest
{
    public class BufferSegment
    {
        public byte[] Buffer { get; set; }
        public int Position { get; set; } = 0;
        public bool IsFinished
        {
            get
            {
                return Position == Buffer.Length;
            }
        }

        public byte ReadFirstByte()
        {
            byte result = Buffer[Position];
            Position++;
            return result;
        }

        public byte[] ReadBufferSegment(int count, out int readCount)
        {
            if (count > Buffer.Length)
            {
                byte[] result = Buffer.Skip(Position).ToArray();
                readCount = result.Length;
                Position = Buffer.Length;
                return result;
            }
            else
            {
                byte[] result = Buffer.Skip(Position).Take(count).ToArray();
                readCount = result.Length;
                Position += readCount;
                return result;
            }
        }
    }

    public class PipeNetworkStream
    {
        private NetworkStream Stream { get; set; }
        private int BufferToRead { get; set; }
        public PipeNetworkStream(NetworkStream stream, int bufferToRead = 512)
        {
            Stream = stream;
            BufferToRead = bufferToRead;
        }

        private BlockingCollection<BufferSegment> BlockBuffers = new BlockingCollection<BufferSegment>();
        private ConcurrentQueue<BufferSegment> QueueBuffers = new ConcurrentQueue<BufferSegment>();

        public Task WriteToStream(byte[] data)
        {
            return Stream.WriteAsync(data, 0, data.Length);
        }

        private async void ReadBuffer()
        {
            //return;
            //byte[] buffer = new byte[BufferToRead];
            //int readCount = await Stream.ReadAsync(buffer, 0, buffer.Length);
            //if (readCount == 0)
            //    throw new Exception("read zero buffer! client disconnected: " + readCount);
            //else if (readCount != buffer.Length)
            //{
            //    Array.Resize(ref buffer, readCount);
            //}
            //Buffers.Enqueue(new BufferSegment() { Buffer = buffer });
        }

        public byte[] Read(int count, out int readCount)
        {
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                ReadBuffer();
                result = BlockBuffers.Take();
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return Read(count, out readCount);
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return Read(count, out readCount);
            }
            else
            {
                byte[] bytes = result.ReadBufferSegment(count, out readCount);
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                return bytes;
            }
        }

        public byte ReadOneByte()
        {
            ReadBuffer();
            BufferSegment result = null;
            if (QueueBuffers.IsEmpty)
            {
                ReadBuffer();
                result = BlockBuffers.Take();
                QueueBuffers.Enqueue(result);
            }
            else
            {
                if (!QueueBuffers.TryPeek(out result))
                    return ReadOneByte();
            }

            if (result.IsFinished)
            {
                QueueBuffers.TryDequeue(out result);
                return ReadOneByte();
            }
            else
            {
                byte b = result.ReadFirstByte();
                if (result.IsFinished)
                    QueueBuffers.TryDequeue(out result);
                return b;
            }
        }
    }


    [ServiceContract("HealthFamilyClientService", ServiceType.ClientService)]
    public interface ITestClientService
    {
        Task<int> CallMe(string data);
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
    }

    [ServiceContract("HealthFamilyService", ServiceType.HttpService)]
    [ServiceContract("HealthFamilyService", ServiceType.ServerService)]
    public interface ITestManager
    {
        bool HelloWorld(string userName, string password);
        Task<bool> HelloWorldAsync(string userName, string password);
        string Test();
    }

    public partial class TestService : ITestManager
    {
        public bool HelloWorld(string userName, string password)
        {
            //OperationContext<UserInfo>.CurrentSetting = new UserInfo() { Name = userName };
            return true;
        }

        public Task<bool> HelloWorldAsync(string userName, string password)
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

        [HttpKey(IsExpireField = true)]
        public DateTime ExpireDateTime { get; set; }
    }


    internal class Program
    {

        private static void Main(string[] args)
        {
            try
            {
                //PipeNetworkStream pipeNetworkStream = new PipeNetworkStream(null);
                //byte[] result = pipeNetworkStream.Read(100, out int readCount);
                //Thread thread = new Thread(() =>
                //{
                //    ServerProvider serverProvider = new ServerProvider();
                //    serverProvider.RegisterServerService(typeof(TestService));
                //    serverProvider.RegisterClientService(typeof(ITestClientService));
                //    serverProvider.Start("http://localhost:3284/TestServices/SignalGo");
                //});
                //thread.Start();
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
                //var result5 = service2.HelloWorld2("reza123", "passee");
                //var result4 = service2.Test();
                //result2 = service.Test();
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
            bool result = await service.HelloWorldAsync("ali123", "passee");
        }

        //public static void PiplineTest()
        //{
        //    TcpListener tcpListener = new TcpListener(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 4545);

        //    tcpListener.Start();

        //    var socket = tcpListener.AcceptSocket();
        //}
    }
}
