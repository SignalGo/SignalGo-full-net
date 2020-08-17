# Publisher
Publisher and Service management tools, is free software derived from the Free and Open source library called Signalgo.  
The main purpose of this line of thought is to make things easier. solve the problems of developers, project managers and help the free and open source software development community. So that everyone can use it any way they want.

## The latest stable version is version 2. In the Publisher Branch.
And the developing version is version 3. In the Publisher3 Branch.

## Some Features:

1. Update your projects from source control like git. to get latest version/changes
2. Compile and get different release of projects.
3. Run Unit Test's.
4. Publish and get outputs for any Linux and windows platforms.
5. Upload to any server that has a server manager or service manager (for Linux and command-line operating systems)
6. Manage services on servers through Publisher:

     6.1 Manage the execution of services
     
          Start, stop, restart
          
     6.2 Get their Health Check status:
     
          Get photos from application output logs (Beta/preview)
          
          Receive health status and availability reports (under development in v3)
          
     6.3 Fetch their files and Edit them remotely. 

## With Security Measures:
     
     In order to be able to communicate with server managers or publish projects on themو you must have the key to that project or service (Both must be the same).
	
## Screenshots
*Define Projects and change settings like key, path, update ignore files ...
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/AddProjectPageView_AndManagmentMenu.png)
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/ProjectInfoView_ProjectSettingsTab.png)
*Command Runner, Run Multiple Queued Commands and get real-time reports:
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/ProjectInfoView_PublishToProtectedServers.png)
*Define and Manage remote servers info (server managers), protect an server with a password
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/ServersManagmentPageView_EditServer.png)
*Get health status and Manage service files on servers:
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/ServiceStatusCheckAndFileManagerView_GetRemotePhoto.png)
*Control Services on servers
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/ServerServiceManagmentView_StopAnService.png)
*Publisher Settings and Commands Config
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/Publisher/PublisherSettingsPage_CommandsSetting.png)

# Server/Service Manager

Server Manager is a tool for managing services on the server side. Which can centrally monitor and manage services/programs and etc.
This software prepares programs to host and work. Stay tuned to a specific port to receive commands or send reports. (With security measures)
It is still needed to publish and manage projects on servers through Publisher.

## Some Features:

1. Add different services / applications
2. Manage their execution status, determine automatic or manual execution during startup (Auto Start)
3. Manage their files (via file manager tab) or access to storage location
4. Determine the time interval between running each service (Delay)
5. Display the amount of memory consumed per each program/process
6. Display the console and output of running programs in separate tabs on the program page itself
7. Change Application Settings, Set specified endpoint address and port (for listening). (The default is on localhost and port 6464)

## Screenshots

* Program/Service Info on Server Manager
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/ServerManager/ServerInfoPageView_Main.png)
* Program Output and logs
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/ServerManager/ServerInfoPageView_ConsoleLogs.png)
* Server Management App Settings
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/ServerManager/ServerManagerSettingsView.png)
* Cross-Platform Command-Line Service Manager
![ScreenShot](https://github.com/saeedrezayi/SignalGo-Publisher/blob/Publisher3/Documents/Images/ConsoleServiceManager/ServiceManager_MainView.png)

# Signal Go

SignalGo is a library for Cross-Platform developers that makes it incredibly simple and easy to add real-time web functionality to your applications. What is "real-time web" functionality? It's the ability to have your server-side code push content to the connected clients as it happens, in real-time. like WCF and SignalR but in a lot easier way and with far more embedded features!

# Why Signal Go?

SignalGo has a lot of features but it's very easy to use. For example, SignalGo has a visual studio extension to generate all you need client side: you don't need to write 1 line of code! No need to create models, enums, services, methods, etc... everything is automatically done for you!
SignalGo has its own very fast json-based protocol and supports also http and https protocols. We prepared a SignalGo test application (with WPF UI) to let you test your server-side methods without writing any code client side.
SignalGo is designed as a RAD tool (rapid application development) keeping always in mind these simple goals:
1. Easy of use
2. Minimal code to write to set up a full working server-client platform
3. Speed 
4. Completeness: exchange almost everything (methods, complex objects, streaming’s, files etc.)
3. Reliability and scalability
4. Security

Is in continuous development with always new cool features you can suggest us too!

## Features:

1. Send and receive any data like class, object, struct and complex objects

2. Send (upload) and receive (download) file streams (audio, video, binary data etc.)

3. Send and receive data with UDP connection for sound and video streaming	

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

     13.1 Support to generate C# client, Angular, C# Blazor, Java, Swift and SOAP Web services.

14. Hosted fully in IIS via Owin

15. Support for duplex client-server service providers

16. with two line of code make your server as a telegram.bot without any changes

17. support validation rule system easy and powerful

...and other features!


## Simple Server Usage:

https://github.com/SignalGo/SignalGo-full-net/wiki/Signalgo-server-HelloWorld


## Simple Client Usage:

https://github.com/SignalGo/signalgo-samples/tree/master/CSharp%20Client%20Sample/CSharpClientSample

### [examples](https://github.com/SignalGo/csharp-sample)

## [SignalGo fully test your server services with a test client in windows, Linux and mac](https://github.com/SignalGo/SignalGoTest)

![ScreenShot](https://raw.githubusercontent.com/SignalGo/SignalGoTest/version4/image3.png "signal go test image")

## Install package from nuget:

     Install-Package SignalGo.Net.Server

     Install-Package SignalGo.Net.Client

     Install-Package SignalGo.JavaScript.Client

# Wiki and Nuget
[![NuGet](https://img.shields.io/badge/github-full%20wiki-brightgreen.svg)](https://github.com/SignalGo/SignalGo-full-net/wiki)
[![NuGet](https://img.shields.io/badge/nuget-server.net%20v5+-blue.svg)](https://www.nuget.org/packages/SignalGo.Net.Server/)
[![NuGet](https://img.shields.io/badge/nuget-client.net%20v5+-blue.svg)](https://www.nuget.org/packages/SignalGo.Net.Client/) 
[![NuGet](https://img.shields.io/badge/nuget-javascript%20v5+-blue.svg)](https://www.nuget.org/packages/SignalGo.JavaScript.Client/)

# Wanna make Publisher and SignalGo better? or you wanna new features etc?
Just create new issues, we will help you as much as we can.

In Telegram:
@mrgrayhat

@Ali_Visual_Studio

Via Email:
mr.grayhatt@gmail.com

ali.visual.studio[AT]gmail.com

## Other source on GitHub
https://github.com/SignalGo


# Maintained By
[Saeed Rezayi](https://github.com/mrgrayhat)

[Ali Yousefi](https://github.com/Ali-YousefiTelori)

