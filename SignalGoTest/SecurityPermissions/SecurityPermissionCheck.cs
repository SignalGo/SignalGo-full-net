using SignalGoTest2Services.Interfaces;
using SignalGoTest2Services.ServerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SignalGoTest.SecurityPermissions
{
    public class SecurityPermissionCheck
    {
        /// <summary>
        //  admin user test
        /// </summary>
        /// <param name="service"></param>
        public void TestAdminUserSync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAdminAccess = service.AdminAccess();
            Assert.True(!resultAdminAccess.IsSuccess && resultAdminAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = service.UserAccess();
            Assert.True(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = service.GustAccess();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = service.Login("test", "test");
            Assert.True(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameAmIGust = service.WhatIsMyName();
            Assert.True(whatIsMyNameAmIGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> adminLoginResult = service.Login("admin", "123");
            Assert.True(adminLoginResult.IsSuccess && adminLoginResult.Data.IsAdmin && adminLoginResult.Data.FullName == "admin user");
            HandleHttpSessions(service);

            string adminWhatIsMyNameAmIGust = service.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");


            userAccess = service.UserAccess();
            Assert.True(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = service.GustAccess();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAdminAccess = service.AdminAccess();
            Assert.True(resultAdminAccess.IsSuccess && resultAdminAccess.Message == "admin success");
        }

        public async Task TestAdminUserASync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAdminAccess = await service.AdminAccessAsync();
            Assert.True(!resultAdminAccess.IsSuccess && resultAdminAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = await service.UserAccessAsync();
            Assert.True(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = await service.GustAccessAsync();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = await service.LoginAsync("test", "test");
            Assert.True(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameAmIGust = await service.WhatIsMyNameAsync();
            Assert.True(whatIsMyNameAmIGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> adminLoginResult = await service.LoginAsync("admin", "123");
            Assert.True(adminLoginResult.IsSuccess && adminLoginResult.Data.IsAdmin && adminLoginResult.Data.FullName == "admin user");
            HandleHttpSessions(service);

            string adminWhatIsMyNameAmIGust = await service.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");


            userAccess = await service.UserAccessAsync();
            Assert.True(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = await service.GustAccessAsync();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAdminAccess = await service.AdminAccessAsync();
            Assert.True(resultAdminAccess.IsSuccess && resultAdminAccess.Message == "admin success");
        }

        public void TestNormalUserSync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAccess = service.AdminAccess();
            Assert.True(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = service.UserAccess();
            Assert.True(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = service.GustAccess();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = service.Login("test", "test");
            Assert.True(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameGust = service.WhatIsMyName();
            Assert.True(whatIsMyNameGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> normaloginResult = service.Login("user", "321");
            Assert.True(normaloginResult.IsSuccess && !normaloginResult.Data.IsAdmin && normaloginResult.Data.IsUser);
            HandleHttpSessions(service);

            string normalWhatIsMyNameGust = service.WhatIsMyName();
            Assert.True(normalWhatIsMyNameGust == "normal user");


            userAccess = service.UserAccess();
            Assert.True(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = service.GustAccess();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAccess = service.AdminAccess();
            Assert.True(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");
        }

        public async Task TestNormalUserASync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAccess = await service.AdminAccessAsync();
            Assert.True(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = await service.UserAccessAsync();
            Assert.True(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = await service.GustAccessAsync();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = await service.LoginAsync("test", "test");
            Assert.True(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameGust = await service.WhatIsMyNameAsync();
            Assert.True(whatIsMyNameGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> normaloginResult = await service.LoginAsync("user", "321");
            Assert.True(normaloginResult.IsSuccess && !normaloginResult.Data.IsAdmin && normaloginResult.Data.IsUser);
            HandleHttpSessions(service);

            string normalWhatIsMyNameGust = await service.WhatIsMyNameAsync();
            Assert.True(normalWhatIsMyNameGust == "normal user");


            userAccess = await service.UserAccessAsync();
            Assert.True(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = await service.GustAccessAsync();
            Assert.True(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAccess = await service.AdminAccessAsync();
            Assert.True(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");
        }

        [Fact]
        public void NormalSignalGoTest()
        {
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            TestAdminUserSync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            TestNormalUserSync(serviceNormal);

            string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            Assert.True(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");
        }

        [Fact]
        public void NormalSignalGoTest2()
        {
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerService<AuthenticationService>(clientAdmin);
            TestAdminUserSync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerService<AuthenticationService>(clientNormal);
            TestNormalUserSync(serviceNormal);

            string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            Assert.True(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");
        }

        [Fact]
        public async Task NormalSignalGoTest2Async()
        {
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerService<AuthenticationService>(clientAdmin);
            await TestAdminUserASync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerService<AuthenticationService>(clientNormal);
            await TestNormalUserASync(serviceNormal);

            string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            Assert.True(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");
        }

        [Fact]
        public async Task NormalSignalGoTestAsync()
        {
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            await TestAdminUserASync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            await TestNormalUserASync(serviceNormal);

            string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            Assert.True(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");
        }

        [Fact]
        public void OneWaySignalGoTest()
        {
            //GlobalInitalization.Initialize();
            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //TestAdminUserSync(serviceAdmin);

            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceNormal = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //TestNormalUserSync(serviceNormal);

            //string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            //Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            //string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            //Assert.True(normalWhatIsMyNameGust == "normal user");

            //adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            //Assert.True(adminWhatIsMyNameAmIGust == "admin user");
        }

        [Fact]
        public async Task OneWaySignalGoTestAsync()
        {
            //GlobalInitalization.Initialize();
            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //await TestAdminUserASync(serviceAdmin);

            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceNormal = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //await TestNormalUserASync(serviceNormal);

            //string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            //Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            //string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            //Assert.True(normalWhatIsMyNameGust == "normal user");

            //adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            //Assert.True(adminWhatIsMyNameAmIGust == "admin user");
        }

        public void HandleHttpSessions(IAuthenticationService service)
        {
            if (service is SignalGoTest2Services.HttpServices.AuthenticationService httpService)
            {
                string cookie = httpService.ResponseHeaders["set-cookie"];
                httpService.RequestHeaders["cookie"] = cookie;
            }
        }

        [Fact]
        public void HttpSignalGoTest()
        {
            SignalGoTest2Services.HttpServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            TestAdminUserSync(serviceAdmin);

            SignalGoTest2Services.HttpServices.AuthenticationService serviceNormal = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            TestNormalUserSync(serviceNormal);

            string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            Assert.True(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            Thread.Sleep(5000);
            //check session expire
            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.True(adminWhatIsMyNameAmIGust == "Gust");

            normalWhatIsMyNameGust = serviceAdmin.WhatIsMyName();
            Assert.True(normalWhatIsMyNameGust == "Gust");
        }

        [Fact]
        public async Task HttpSignalGoTestAsync()
        {
            SignalGoTest2Services.HttpServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            await TestAdminUserASync(serviceAdmin);

            SignalGoTest2Services.HttpServices.AuthenticationService serviceNormal = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            await TestNormalUserASync(serviceNormal);

            string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            Assert.True(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "admin user");

            Thread.Sleep(5000);
            //check session expire
            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(adminWhatIsMyNameAmIGust == "Gust");

            normalWhatIsMyNameGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.True(normalWhatIsMyNameGust == "Gust");
        }
    }
}
