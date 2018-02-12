using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest.Models;

namespace SignalGoTest.DataExhanger
{
    [TestClass]
    public class DataExchangerTest
    {
        [TestMethod]
        public void TestDataExchanger()
        {
            GlobalInitalization.Initialize();
            var client = GlobalInitalization.InitializeAndConnecteClient();
            var service = client.RegisterClientServiceInterfaceWrapper<ITestServerModel>();

            var users = service.GetListOfUsers();
            Assert.IsTrue(string.IsNullOrEmpty(users[0].Password));
            Assert.IsTrue(users[0].LastPostInfo == null);
            Assert.IsTrue(users[0].PostInfoes != null);
            Assert.IsTrue(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.IsTrue(users[0].PostInfoes[0].User == null);

            var posts = service.GetPostsOfUser(1);
            Assert.IsTrue(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.IsTrue(posts[0].User == null);

            var customPosts = service.GetCustomPostsOfUser(1);
            Assert.IsTrue(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.IsTrue(customPosts[0].User == null);
            Assert.IsTrue(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            var customUsers = service.GetListOfUsersCustom();
            Assert.IsTrue(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.IsTrue(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.IsTrue(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            var loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.IsTrue(loginResult);
        }
    }
}
