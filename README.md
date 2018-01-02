# [You can read Wiki by click here](https://github.com/SignalGo/SignalGo-full-net/wiki)

# Signal Go

SignalGo is a library for Cross-Platform developers that makes it incredibly simple and easy to add real-time web functionality to your applications. What is "real-time web" functionality? It's the ability to have your server-side code push content to the connected clients as it happens, in real-time. like WCF and SignalR


## Features:
1.Send and receive any data like class,object,struct and complex object

2.Send(upload) and receive(download) stream like file stream

3.Send and receive data with UDP connection for sound and video streamming

4.Return data from a method (client and server)

5.Linq query to Clients for send or receive data

6.Setting for maximum and minimum and timeout data transport

7.Using best security algoritm for send or receive data

8.call methods from http GET and POST protocol (like call from browser) or upload and download files and manage Controllers like asp.net MVC

9.support async await methods
     
10.full logging systeam
     
11.manage data exchanger to customize model properties in send and receive data without create new class

12.Ip limitation for call methods

13.easy to manage permissions with attribute you can customize your permissions before client call methods

14.Automatic handle object references and pointers in serialize and deserialize system

and other fetures...

After you learn it [ServiceContractAttribute](https://github.com/SignalGo/SignalGo-full-net/wiki/ServiceContract-(Attribute)) you can create your simple Service with this attribute.

for example we have an interface that is our sevice level methods.

```csharp
    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel")]
    public interface ITestServerModel
    {
        Tuple<string> HelloWorld(string yourName);
    }

    public class TestServerModel : ITestServerModel
    {
        public Tuple<string> HelloWorld(string yourName)
        {
            return new Tuple<string>("hello: " + yourName);
        }
    }
```

# How to start  my service?

After create your service classes you must start your service listener in your Main method console project or your windows service project or ...

for example:

```csharp
static void Main(string[] args)
        {
            // create instance of your server listener
            SignalGo.Server.ServiceManager.ServerProvider server = new SignalGo.Server.ServiceManager.ServerProvider();
            //register your service class that have implemented methods (not interfaces)
            server.InitializeService<TestServerModel>();
            //start your server provider (your server address is important for client to connect)
            server.Start("http://localhost:1132/SignalGoTestService");
            //this code hold windows close and don't let him to close after read one line this will be close.
            Console.ReadLine();
        }
```

## Client Providers (Client Side Example)

After create your [server side project](Service-Providers-(Server-Side-Example)) you must create your client side project for connect to your server.

In the client side you just need your interface service not your service class that have implemented methods.
So I think this is better for you if your service interface project be separated from your service class projects because you dont need create your service interfaces again for your client side this will make easy way for you to manage your services when you just add reference from service interface project to your client project.

```csharp
                //your client connector that will be connect to your server
                ClientProvider provider = new ClientProvider();
                //connect to your server must have full address that your server is listen
                provider.Connect("http://localhost:1132/SignalGoTestService");
                //register your service interfacce for client
                var testServerModel = provider.RegisterClientServiceInterfaceWrapper<ITestServerModel>();
                //call server method and return value from your server to client
                var result = testServerModel.HelloWorld("ali");
                //print your result to console
                Console.WriteLine(result.Item1);
```

SignalGo have another way to register your service interface like:

```csharp
var testServerModel = provider.RegisterClientServiceDynamic<ITestServerModel>();
//or
var testServerModel = provider.RegisterClientServiceDynamic("TestServerModel");
```

### [You can read more Wiki by click here](https://github.com/SignalGo/SignalGo-full-net/wiki)

### [C# sample source project](https://github.com/SignalGo/csharp-sample)


## How to use security setting (just C# .net version is currently available)?

```csharp
            ClientProvider connector = new ClientProvider();
            connector.Connect("http://localhost:1199/SignalGoTestServicesProject");
            var callbacks = connector.RegisterServerCallback<ClientCallback>();
            var service = connector.RegisterClientServiceInterface<ISignalGoServerMethods>();
            connector.SetSecuritySettings(new SignalGo.Shared.Models.SecuritySettingsInfo() { SecurityMode = SignalGo.SecurityMode.RSA_AESSecurity });
                
```

## How to send and receive UDP data?



#### server-side:

```csharp
    server.ConnectToUDP(port);
```
#### client-side:
```csharp
        connector.ConnectToUDP("your ip", port);
        connector.OnReceivedData = (bytes) =>
        {
            Console.WriteLine("RECEIVED BYTES UDP :" + bytes.Length);
        };

        connector.SendUdpData(new byte[] { 1, 2, 3, 40 });
```

## How to usage http calls and manage controllers (download or upload files and methods)?


#### server-side:

```csharp
    [HttpSupport("AddressTest")]
    public class SimpleHttpRequest : HttpRequestController
    {
        public ActionResult DownloadImage(string name, int num)
        {
            if (num <= 0)
            {
                Status = System.Net.HttpStatusCode.Forbidden;
                return Content("num is not true!");
            }
            ResponseHeaders.Add("Content-Type", "image/jpeg");
            return new FileActionResult(@"D:\photo_2017-03-08_00-45-04.jpg");
        }
        
        public ActionResult Hello(string name)
        {
            return Content("hello:" + name);
        }
        
        public ActionResult TestUploadFile(Guid token, int profileId)
        {
            var fileInfo = TakeNextFile();
            if (fileInfo == null)
            {
                Status = System.Net.HttpStatusCode.NotFound;
                return Content("file not found!");
            }
            using (var streamWriter = new FileStream("D:\\testfileName.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var bytes = new byte[1024 * 10];
                while (true)
                {
                    var readCount = fileInfo.InputStream.Read(bytes, 0, bytes.Length);
                    if (readCount <= 0)
                        break;
                    streamWriter.Write(bytes, 0, readCount);
                }
                long fileLen = streamWriter.Length;
            }
            return Content("success!");
        }
    }
```

after create your controller class you must register that in to your server after start like:

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SignalGo.Server.ServiceManager.ServerProvider();
            server.Start("http://localhost:1199/SignalGoTestServicesProject");
            server.InitializeService(typeof(SignalGoServerMethods));
            server.RegisterClientCallbackInterfaceService<ISignalGoClientMethods>();
            server.AddHttpService(typeof(SimpleHttpRequest));
        }
    }
```

after that you can call your methods from this addresses:

http://localhost:1199/AddressTest/DownloadImage?ali&12

http://localhost:1199/AddressTest/Hello?ali

## [Sample-Source](https://github.com/SignalGo/csharp-sample)
## [SignalGo-Test Methods (like wcf test)](https://github.com/SignalGo/SignalGoTest)

![ScreenShot](https://github.com/SignalGo/SignalGoTest/blob/master/image2.png "signal go test image")

## Install package from nuget:

Install-Package SignalGo.Net.Server

Install-Package SignalGo.Net.Client

Install-Package SignalGo.JavaScript.Client

# Pull Requests
I welcome all pull requests from you guys.Here are 3 basic rules of your request:
  1. Match coding style (braces, spacing, etc.)
  2. If its a feature, bugfix, or anything please only change code to what you specify.
  3. Please keep PR titles easy to read and descriptive of changes, this will make them easier to merge :)

  
## Other source on github
  1. [Java Script Client](https://github.com/SignalGo/client-js)
  2. [Java Client](https://github.com/SignalGo/client-java)
  

# Maintained By
[Ali Yousefi](https://github.com/Ali-YousefiTelori)

[Blog](http://framesoft.ir)
