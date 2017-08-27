# server-net
Server version of SignalGo for .Net Framework

SignalGo is a library for Cross-Platform developers that makes it incredibly simple to add real-time web functionality to your applications. What is "real-time web" functionality? It's the ability to have your server-side code push content to the connected clients as it happens, in real-time. like WCF and SignalR


## Features:
1.Send and receive any data like class,object,parameters,methods,return types

2.Send(upload) and receive(download) stream like file stream

3.Send and receive data with UDP connection for sound and video streamming

4.Return data from a method (client and server)

5.Linq query to Clients for send or receive data

6.Setting for maximum and minimum and timeout data transport

7.Using best security algoritm for send or receive data

8.call methods from http GET protocol(like browser) and manage Controllers like asp.net MVC

and other fetures...


### Quick Usage Client-Side:

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            ClientProvider connector = new ClientProvider();
            connector.Connect("http://localhost:1199/SignalGoTestServicesProject");
            var callbacks = connector.RegisterServerCallback<ClientCallback>();
            var service = connector.RegisterClientServiceInterface<ISignalGoServerMethods>();
            var result = service.Login("admin", "admin");

        }
    }

    [ServiceContract("SignalGoTestClientService")]
    public class ClientCallback
    {
        public ConnectorBase Connector
        {
            get
            {
                return OperationContract.GetConnector<ConnectorBase>(this);
            }
        }

        public string GetMeSignalGo(string value)
        {
            return "GetMeSignalGo :" + value;
        }

        public void HelloSignalGo(string hello)
        {
        }
    }

    [ServiceContract("SignalGoTestService")]
    public interface ISignalGoServerMethods
    {
        bool Login(string userName, string password);
    }

```

### Quick Usage Server-Side:

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SignalGo.Server.ServiceManager.ServerProvider();
            server.Start("http://localhost:1199/SignalGoTestServicesProject");
            server.InitializeService(typeof(SignalGoServerMethods));
            server.RegisterClientCallbackInterfaceService<ISignalGoClientMethods>();
        }
    }

    [ServiceContract("SignalGoTestService")]
    public class SignalGoServerMethods
    {
        public ISignalGoClientMethods callback = null;
        OperationContext currentContext = null;
        public SignalGoServerMethods()
        {
            currentContext = OperationContext.Current;
            callback = currentContext.GetClientCallback<ISignalGoClientMethods>();
        }

        public bool Login(string userName, string password)
        {

            //get all clients call interface list
            foreach (var call in currentContext.GetAllClientCallbackList<ISignalGoClientMethods>())
            {
                //call GetMeSignalGo method
                var result = call.GetMeSignalGo("test");

                //call HelloSignalGo method
                call.HelloSignalGo("hello signalGo");
            }

            if (userName == "admin" && password == "admin")
                return true;
            return false;
        }
    }

    [ServiceContract("SignalGoTestClientService")]
    public interface ISignalGoClientMethods
    {
        void HelloSignalGo(string hello);
        string GetMeSignalGo(string value);
    }
```
## How to send and receive stream?

for download a file from server you must add a method that must return StreamInfo class for example:

#### server-side:

```csharp
        public StreamInfo DownloadFile(string fileName)
        {
            var streamInfo = new StreamInfo() { Headers = new Dictionary<string, object>() { { "Size", 1024 } }};
            streamInfo.Stream = new FileStream(@"D:\SignalGo-Sample.rar", FileMode.Open, FileAccess.Read);
            return streamInfo;
        }
```

#### client-side:
```csharp
        using (var stream = service.DownloadFile("test message"))
        {
            ///code to read stream
        }
```

for upload a file to server you must add a method that not have return type (is void) and must have one parameter that type is StreamInfo for example:

#### server-side:

```csharp
        public void UploadFile(StreamInfo streamUpload)
        {
            byte[] dataRead = new byte[1024];
            var readCount = streamUpload.Stream.Read(dataRead, 0, dataRead.Length);
            var rrr = streamUpload.Stream.ReadByte();
            Console.WriteLine("server read uploaded bytes: " + readCount);
        }
```

#### client-side:
```csharp
        using (StreamInfo stream = new StreamInfo() { Headers = new Dictionary<string, object>() { { "size", 1000 } } })
        {
            service.UploadFile(stream);
            var bytesToUpload = new byte[] { 1, 2, 3, 4, 5, 10 };
            stream.Stream.Write(bytesToUpload, 0, bytesToUpload.Length);
        }
```

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
        public FileActionResult DownloadImage(string name, int num)
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
