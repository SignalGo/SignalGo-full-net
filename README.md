# [You can read Wiki by click here](https://github.com/SignalGo/SignalGo-full-net/wiki)

# Signal Go

SignalGo is a library for Cross-Platform developers that makes it incredibly simple and easy to add real-time web functionality to your applications. What is "real-time web" functionality? It's the ability to have your server-side code push content to the connected clients as it happens, in real-time. like WCF and SignalR


## Features:

1. Send and receive any data like class, object, struct and complex objects

2. Send (upload) and receive (download) file streams (audio, video, binary data etc.)

3. Send and receive data with UDP connection for sound and video streamming

4. Return data from a method (both client and server)

5. Linq query to clients to send or receive data

6. Setting the maximum and minimum byte size for data transport and connection timeout too.

7. Using the best security algorithm to send and receive data

8. Call methods from http GET and POST protocol (like browser's call) or upload and download files and manage controllers like asp.net MVC

9. Full support for "async... await" methods
     
10. Manage data exchanger to customize model properties to send and receive data without create new classes

11. Ip limitations for call methods

12. Easy to manage permissions with attributes. This way you can customize your permissions before client call methods

13. Automatic handle object references and pointers for the serialize - deserialize system

and other features...

## Simple Usage:

After you learn this lesson: [ServiceContractAttribute](https://github.com/SignalGo/SignalGo-full-net/wiki/ServiceContract-(Attribute)) you can create your simple service with this attribute.

For example we have an interface that is our service level methods.

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

# How to start my service?

After you created your service class you must start your service listener in your Main method console project or your windows service project or web project ...

For example:

```csharp
static void Main(string[] args)
        {
            //Create an instance of your server listener
            SignalGo.Server.ServiceManager.ServerProvider server = new SignalGo.Server.ServiceManager.ServerProvider();
            //Register your service class where you have implemented the methods (not interfaces)
            server.InitializeService<TestServerModel>();
            //Start your server provider (server address is mandatory for client to connect)
            server.Start("http://localhost:1132/SignalGoTestService");
            //This code handles windows console app to close after you press a key
            Console.ReadLine();
        }
```

## Client Providers (Client Side Example)

After you create your [server side project](Service-Providers-(Server-Side-Example)) you must create your client side project to connect to your server.

Client side you only need your interface service, not your service class where you have implemented methods.
Having your service interface separated from your service class projects is better. In fact you dont need create your service interfaces again for your client. This is a very easy way to manage your services because in the client you only need to reference the interface project.

```csharp
                //The client connector that will connect to your server
                ClientProvider provider = new ClientProvider();
                //Connect to server to a valid address where server is listening on
                provider.Connect("http://localhost:1132/SignalGoTestService");
                //Register the service interface in the client
                var testServerModel = provider.RegisterClientServiceInterfaceWrapper<ITestServerModel>();
                //Call server method and return the result value to the client
                var result = testServerModel.HelloWorld("ali");
                //Print the result on the client console
                Console.WriteLine(result.Item1);
```

SignalGo has another way to register your service interface too:

```csharp
var testServerModel = provider.RegisterClientServiceDynamic<ITestServerModel>();
//or
var testServerModel = provider.RegisterClientServiceDynamic("TestServerModel");
```

### [You can read more Wiki by click here](https://github.com/SignalGo/SignalGo-full-net/wiki)

### [C# sample source project](https://github.com/SignalGo/csharp-sample)


## How to use security settings? (Please consider that only C# .net version is currently available)

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

## How to manage http calls and manage controllers (methods and download-upload files)?


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

After you created your controller class you must register service interface like this:

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

Then you can call your methods from this addresses like the following example:

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
I welcome all pull requests from you guys! PLease folow these 3 basic rules:
  1. Match coding style (braces, spacing, etc.)
  2. If its a feature, bugfix, or anything please only change code to what you specify.
  3. Please keep PR titles easy to read and descriptive of changes, this will make them easier to merge :)

  
## Other source on github
  1. [Java Script Client](https://github.com/SignalGo/client-js)
  2. [Java Client](https://github.com/SignalGo/client-java)
  

# Maintained By
[Ali Yousefi](https://github.com/Ali-YousefiTelori)

[Blog](http://framesoft.ir)
