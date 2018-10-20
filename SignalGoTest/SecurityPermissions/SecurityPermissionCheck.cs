using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2Services.Interfaces;
using SignalGoTest2Services.ServerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SignalGoTest.SecurityPermissions
{
    [TestClass]
    public class SecurityPermissionCheck
    {
        /// <summary>
        //  admin user test
        /// </summary>
        /// <param name="service"></param>
        public void TestAdminUserSync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAdminAccess = service.AdminAccess();
            Assert.IsTrue(!resultAdminAccess.IsSuccess && resultAdminAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = service.UserAccess();
            Assert.IsTrue(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = service.GustAccess();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = service.Login("test", "test");
            Assert.IsTrue(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameAmIGust = service.WhatIsMyName();
            Assert.IsTrue(whatIsMyNameAmIGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> adminLoginResult = service.Login("admin", "123");
            Assert.IsTrue(adminLoginResult.IsSuccess && adminLoginResult.Data.IsAdmin && adminLoginResult.Data.FullName == "admin user");
            HandleHttpSessions(service);

            string adminWhatIsMyNameAmIGust = service.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");


            userAccess = service.UserAccess();
            Assert.IsTrue(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = service.GustAccess();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAdminAccess = service.AdminAccess();
            Assert.IsTrue(resultAdminAccess.IsSuccess && resultAdminAccess.Message == "admin success");
        }

        public async Task TestAdminUserASync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAdminAccess = await service.AdminAccessAsync();
            Assert.IsTrue(!resultAdminAccess.IsSuccess && resultAdminAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = await service.UserAccessAsync();
            Assert.IsTrue(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = await service.GustAccessAsync();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = await service.LoginAsync("test", "test");
            Assert.IsTrue(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameAmIGust = await service.WhatIsMyNameAsync();
            Assert.IsTrue(whatIsMyNameAmIGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> adminLoginResult = await service.LoginAsync("admin", "123");
            Assert.IsTrue(adminLoginResult.IsSuccess && adminLoginResult.Data.IsAdmin && adminLoginResult.Data.FullName == "admin user");
            HandleHttpSessions(service);

            string adminWhatIsMyNameAmIGust = await service.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");


            userAccess = await service.UserAccessAsync();
            Assert.IsTrue(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = await service.GustAccessAsync();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAdminAccess = await service.AdminAccessAsync();
            Assert.IsTrue(resultAdminAccess.IsSuccess && resultAdminAccess.Message == "admin success");
        }

        public void TestNormalUserSync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAccess = service.AdminAccess();
            Assert.IsTrue(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = service.UserAccess();
            Assert.IsTrue(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = service.GustAccess();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = service.Login("test", "test");
            Assert.IsTrue(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameGust = service.WhatIsMyName();
            Assert.IsTrue(whatIsMyNameGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> normaloginResult = service.Login("user", "321");
            Assert.IsTrue(normaloginResult.IsSuccess && !normaloginResult.Data.IsAdmin && normaloginResult.Data.IsUser);
            HandleHttpSessions(service);

            string normalWhatIsMyNameGust = service.WhatIsMyName();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");


            userAccess = service.UserAccess();
            Assert.IsTrue(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = service.GustAccess();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAccess = service.AdminAccess();
            Assert.IsTrue(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");
        }

        public async Task TestNormalUserASync(IAuthenticationService service)
        {
            SignalGoTest2.Models.MessageContract resultAccess = await service.AdminAccessAsync();
            Assert.IsTrue(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract userAccess = await service.UserAccessAsync();
            Assert.IsTrue(!userAccess.IsSuccess && userAccess.Message == "Session access denied!");

            SignalGoTest2.Models.MessageContract gustAccess = await service.GustAccessAsync();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> loginResult = await service.LoginAsync("test", "test");
            Assert.IsTrue(!loginResult.IsSuccess && loginResult.Message == "Username or Password Incorrect!" && loginResult.Data == null);

            string whatIsMyNameGust = await service.WhatIsMyNameAsync();
            Assert.IsTrue(whatIsMyNameGust == "Gust");

            SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.UserInfo> normaloginResult = await service.LoginAsync("user", "321");
            Assert.IsTrue(normaloginResult.IsSuccess && !normaloginResult.Data.IsAdmin && normaloginResult.Data.IsUser);
            HandleHttpSessions(service);

            string normalWhatIsMyNameGust = await service.WhatIsMyNameAsync();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");


            userAccess = await service.UserAccessAsync();
            Assert.IsTrue(userAccess.IsSuccess && userAccess.Message == "user success");

            gustAccess = await service.GustAccessAsync();
            Assert.IsTrue(gustAccess.IsSuccess && gustAccess.Message == "gust success");

            resultAccess = await service.AdminAccessAsync();
            Assert.IsTrue(!resultAccess.IsSuccess && resultAccess.Message == "Session access denied!");
        }

        [TestMethod]
        public void NormalSignalGoTest()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            TestAdminUserSync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            TestNormalUserSync(serviceNormal);

            string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");
        }

        [TestMethod]
        public void NormalSignalGoTest2()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerService<AuthenticationService>(clientAdmin);
            TestAdminUserSync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerService<AuthenticationService>(clientNormal);
            TestNormalUserSync(serviceNormal);

            string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");
        }

        [TestMethod]
        public async Task NormalSignalGoTest2Async()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerService<AuthenticationService>(clientAdmin);
            await TestAdminUserASync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerService<AuthenticationService>(clientNormal);
            await TestNormalUserASync(serviceNormal);

            string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");
        }

        [TestMethod]
        public async Task NormalSignalGoTestAsync()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider clientAdmin = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceAdmin = clientAdmin.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            await TestAdminUserASync(serviceAdmin);

            SignalGo.Client.ClientProvider clientNormal = GlobalInitalization.InitializeAndConnecteClient();
            IAuthenticationService serviceNormal = clientNormal.RegisterServerServiceInterfaceWrapper<IAuthenticationService>();
            await TestNormalUserASync(serviceNormal);

            string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");
        }

        [TestMethod]
        public void OneWaySignalGoTest()
        {
            //GlobalInitalization.Initialize();
            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //TestAdminUserSync(serviceAdmin);

            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceNormal = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //TestNormalUserSync(serviceNormal);

            //string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            //Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            //string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            //Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            //adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            //Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");
        }

        [TestMethod]
        public async Task OneWaySignalGoTestAsync()
        {
            //GlobalInitalization.Initialize();
            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //await TestAdminUserASync(serviceAdmin);

            //SignalGoTest2Services.OneWayServices.AuthenticationService serviceNormal = new SignalGoTest2Services.OneWayServices.AuthenticationService("localhost", 1132);

            //await TestNormalUserASync(serviceNormal);

            //string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            //Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            //string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            //Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            //adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            //Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");
        }

        public void HandleHttpSessions(IAuthenticationService service)
        {
            if (service is SignalGoTest2Services.HttpServices.AuthenticationService httpService)
            {
                string cookie = httpService.ResponseHeaders["set-cookie"];
                httpService.RequestHeaders["cookie"] = cookie;
            }
        }

        [TestMethod]
        public void HttpSignalGoTest()
        {
            GlobalInitalization.Initialize();
            SignalGoTest2Services.HttpServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            TestAdminUserSync(serviceAdmin);

            SignalGoTest2Services.HttpServices.AuthenticationService serviceNormal = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            TestNormalUserSync(serviceNormal);

            string adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = serviceNormal.WhatIsMyName();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            Thread.Sleep(5000);
            //check session expire
            adminWhatIsMyNameAmIGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "Gust");

            normalWhatIsMyNameGust = serviceAdmin.WhatIsMyName();
            Assert.IsTrue(normalWhatIsMyNameGust == "Gust");
        }

        [TestMethod]
        public async Task HttpSignalGoTestAsync()
        {
            GlobalInitalization.Initialize();
            SignalGoTest2Services.HttpServices.AuthenticationService serviceAdmin = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            await TestAdminUserASync(serviceAdmin);

            SignalGoTest2Services.HttpServices.AuthenticationService serviceNormal = new SignalGoTest2Services.HttpServices.AuthenticationService("http://localhost:1132");

            await TestNormalUserASync(serviceNormal);

            string adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            string normalWhatIsMyNameGust = await serviceNormal.WhatIsMyNameAsync();
            Assert.IsTrue(normalWhatIsMyNameGust == "normal user");

            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "admin user");

            Thread.Sleep(5000);
            //check session expire
            adminWhatIsMyNameAmIGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(adminWhatIsMyNameAmIGust == "Gust");

            normalWhatIsMyNameGust = await serviceAdmin.WhatIsMyNameAsync();
            Assert.IsTrue(normalWhatIsMyNameGust == "Gust");
        }
    }
}
