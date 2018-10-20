using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2.Models;
using SignalGoTest2Services.Interfaces;
using SignalGoTest2Services.ServerServices;
using System.Threading.Tasks;

namespace SignalGoTest.DataExhanger
{
    [TestClass]
    public class DataExchangerTest
    {
        [TestMethod]
        public void TestDataExchanger()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            //while (true)
            //    System.Threading.Thread.Sleep(100);
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = service.HelloBind(test, test1, test2);
            Assert.IsTrue(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = service.GetListOfUsers();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = service.GetPostsOfUser(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = service.GetCustomPostsOfUser(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(customPosts.Count == 6);
            Assert.IsTrue(customPosts[5].PostSecurityLink == null);
            Assert.IsTrue(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = service.GetListOfUsersCustom();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }

        [TestMethod]
        public async Task TestDataExchangerAsync()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();

            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = await service.HelloBindAsync(test, test1, test2);
            Assert.IsTrue(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = await service.GetListOfUsersAsync();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = await service.GetPostsOfUserAsync(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = await service.GetCustomPostsOfUserAsync(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(customPosts.Count == 6);
            Assert.IsTrue(customPosts[5].PostSecurityLink == null);
            Assert.IsTrue(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = await service.GetListOfUsersCustomAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = await service.LoginAsync(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }

        [TestMethod]
        public void TestDataExchangerOneWay()
        {
            GlobalInitalization.Initialize();
            SignalGoTest2Services.OneWayServices.TestServerModel service = new SignalGoTest2Services.OneWayServices.TestServerModel("localhost", 1132);
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = service.HelloBind(test, test1, test2);
            Assert.IsTrue(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = service.GetListOfUsers();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = service.GetPostsOfUser(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = service.GetCustomPostsOfUser(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(customPosts.Count == 6);
            Assert.IsTrue(customPosts[5].PostSecurityLink == null);
            Assert.IsTrue(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = service.GetListOfUsersCustom();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }

        [TestMethod]
        public async Task TestDataExchangerOneWayAsync()
        {
            GlobalInitalization.Initialize();
            SignalGoTest2Services.OneWayServices.TestServerModel service = new SignalGoTest2Services.OneWayServices.TestServerModel("localhost", 1132);

            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = await service.HelloBindAsync(test, test1, test2);
            Assert.IsTrue(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = await service.GetListOfUsersAsync();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = await service.GetPostsOfUserAsync(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = await service.GetCustomPostsOfUserAsync(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(customPosts.Count == 6);
            Assert.IsTrue(customPosts[5].PostSecurityLink == null);
            Assert.IsTrue(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = await service.GetListOfUsersCustomAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = await service.LoginAsync(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }


        [TestMethod]
        public void TestDataExchangerHttp()
        {
            GlobalInitalization.Initialize();
            SignalGoTest2Services.HttpServices.TestServerModel service = new SignalGoTest2Services.HttpServices.TestServerModel("http://localhost:1132");
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = service.HelloBind(test, test1, test2);
            Assert.IsTrue(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = service.GetListOfUsers();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = service.GetPostsOfUser(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = service.GetCustomPostsOfUser(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(customPosts.Count == 6);
            Assert.IsTrue(customPosts[5].PostSecurityLink == null);
            Assert.IsTrue(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = service.GetListOfUsersCustom();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }

        [TestMethod]
        public async Task TestDataExchangerHttpAsync()
        {
            GlobalInitalization.Initialize();
            SignalGoTest2Services.HttpServices.TestServerModel service = new SignalGoTest2Services.HttpServices.TestServerModel("http://localhost:1132");

            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = await service.HelloBindAsync(test, test1, test2);
            Assert.IsTrue(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = await service.GetListOfUsersAsync();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = await service.GetPostsOfUserAsync(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = await service.GetCustomPostsOfUserAsync(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(customPosts.Count == 6);
            Assert.IsTrue(customPosts[5].PostSecurityLink == null);
            Assert.IsTrue(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = await service.GetListOfUsersCustomAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = await service.LoginAsync(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }
    }
}
