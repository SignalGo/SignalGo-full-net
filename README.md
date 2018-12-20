# Wiki and Nuget
[![NuGet](https://img.shields.io/badge/github-full%20wiki-brightgreen.svg)](https://github.com/SignalGo/SignalGo-full-net/wiki)
[![NuGet](https://img.shields.io/badge/nuget-server.net%20v5+-blue.svg)](https://www.nuget.org/packages/SignalGo.Net.Server/)  [![NuGet](https://img.shields.io/badge/nuget-client.net%20v5+-blue.svg)](https://www.nuget.org/packages/SignalGo.Net.Client/)  [![NuGet](https://img.shields.io/badge/nuget-javascript%20v5+-blue.svg)](https://www.nuget.org/packages/SignalGo.JavaScript.Client/)

# Signal Go

SignalGo is a library for Cross-Platform developers that makes it incredibly simple and easy to add real-time web functionality to your applications. What is "real-time web" functionality? It's the ability to have your server-side code push content to the connected clients as it happens, in real-time. like WCF and SignalR but in a lot easier way and with far more embedded features!

# Why Signal Go?

SignalGo has a lot of features but it's very easy to use. For example, SignalGo has a visual studio extension to generate all you need client side: you don't need to write 1 line of code! No need to create models, enums, services, methods,etc... everything is automatically done for you!
SignalGo has its own very fast json-based protocol and supports also http and https protocols. We prepared a SignalGo test application (with WPF UI) to let you test your server side methods without writing any code client side.
SignalGo is designed as a RAD tool (rapid application development) keeping always in mind these simple goals:
1. Easy of use
2. Minimal code to write to set up a full working server-client platform
3. Speed 
4. Completeness: exchange almost everything (methods, complex objects, streamings, files etc.)
3. Reliability and scalability
4. Security

Is in continuous development with always new cool features you can suggest us too!

## Features:

1. Send and receive any data like class, object, struct and complex objects

2. Send (upload) and receive (download) file streams (audio, video, binary data etc.)

3. Send and receive data with UDP connection for sound and video streamming

4. Return data from a method (both client and server)

5. Linq query to clients to send or receive data

6. Setting the maximum and minimum byte size for data transport and connection timeout too.

7. Call methods from http GET and POST protocol (like browser's or postman call) or upload and download files and manage controllers like asp.net MVC

8. Full support for "async... await" methods
     
9. Manage data exchanger to customize model properties to send and receive data without create new classes better and easier than GraphQL and OData

10. Ip limitations for call methods

11. Easy to manage permissions with attributes. This way you can customize your permissions before client call methods

12. Automatic handle object references and pointers for the serialize - deserialize system

13. Add service reference and generate models etc. client side directly with the visual studio add-in  [![NuGet](https://img.shields.io/badge/github-wiki-brightgreen.svg)](https://github.com/SignalGo/SignalGo-full-net/wiki/Add-Service-Reference---Auto-generate-all-services-and-models-in-client-side)

     13.1 Support to generate C# client , Angular , C# Blazor and SOAP Web services.

14. Hosted fully in IIS via Owin

15. Support for duplex client-server service providers

16. with two line of code make your server as a telegram.bot without any changes

17.support validation rule system easy and powerful
...and other features!


## Simple Server Usage:

https://github.com/SignalGo/SignalGo-full-net/wiki/Signalgo-server-HelloWorld


## Simple Client Usage:

https://github.com/SignalGo/signalgo-samples/tree/master/CSharp%20Client%20Sample/CSharpClientSample


### [You can read more Wiki by click here](https://github.com/SignalGo/SignalGo-full-net/wiki)

### [examples](https://github.com/SignalGo/csharp-sample)

## [SignalGo fully test your server services with a test client in windows, linux and mac](https://github.com/SignalGo/SignalGoTest)

![ScreenShot](https://raw.githubusercontent.com/SignalGo/SignalGoTest/version4/image3.png "signal go test image")

## Install package from nuget:

Install-Package SignalGo.Net.Server

Install-Package SignalGo.Net.Client

Install-Package SignalGo.JavaScript.Client

# Wanna make SignalGo better? or you wanna new features etc?
Just create new issues I will help you fast.

Call me on Telegram:
@Ali_Visual_Studio

Email:
ali.visual.studio[AT]gmail.com

  
## Other source on github
https://github.com/SignalGo
  

# Maintained By
[Ali Yousefi](https://github.com/Ali-YousefiTelori)

