using SignalGo.Server.Models;
using SignalGo.Shared.Converters;
using SignalGo.Shared.DataTypes;
using SignalGoTest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class TestSetting
    {
        public string Name { get; set; }
    }

    [ServiceContract("Authentication", ServiceType.HttpService, InstanceType.SingleInstance)]
    [ServiceContract("Authentication", ServiceType.OneWayService, InstanceType.SingleInstance)]
    [ServiceContract("Authentication", ServiceType.ServerService, InstanceType.SingleInstance)]
    public class AuthenticationService
    {
        public MessageContract<UserInfo> Login(string userName, string password)
        {
            if (userName == "admin" && password == "123")
            {
                UserInfo user = new UserInfo() { FullName = "admin user", IsAdmin = true, IsUser = true, Password = password, UserName = userName, Id = 1 };
                OperationContext<CurrentUserInfo>.CurrentSetting = new CurrentUserInfo()
                {
                    ExpireDate = DateTime.Now.AddSeconds(5),
                    UserInfo = user,
                    Session = Guid.NewGuid().ToString()
                };
                return new MessageContract<UserInfo>()
                {
                    IsSuccess = true,
                    Data = user
                };
            }
            else if (userName == "user" && password == "321")
            {
                UserInfo user = new UserInfo() { FullName = "normal user", IsAdmin = false, IsUser = true, Password = password, UserName = userName, Id = 2 };
                OperationContext<CurrentUserInfo>.CurrentSetting = new CurrentUserInfo()
                {
                    ExpireDate = DateTime.Now.AddSeconds(5),
                    UserInfo = user,
                    Session = Guid.NewGuid().ToString()
                };
                return new MessageContract<UserInfo>()
                {
                    IsSuccess = true,
                    Data = user
                };
            }
            return new MessageContract<UserInfo>() { IsSuccess = false, Message = "Username or Password Incorrect!" };
        }

        public string WhatIsMyName()
        {
            CurrentUserInfo current = OperationContext<CurrentUserInfo>.CurrentSetting;
            if (current != null)
                return current.UserInfo.FullName;
            return "Gust";
        }

        [TestSecurityPermissions(IsAdmin = true)]
        public MessageContract AdminAccess()
        {
            CurrentUserInfo current = OperationContext<CurrentUserInfo>.CurrentSetting;
            if (current != null && current.UserInfo.IsAdmin)
                return new MessageContract() { IsSuccess = true, Message = "admin success" };
            return new MessageContract() { IsSuccess = false, Message = "wrong called signalgo bug is here" };

        }

        [TestSecurityPermissions(IsUser = true)]
        public MessageContract UserAccess()
        {
            CurrentUserInfo current = OperationContext<CurrentUserInfo>.CurrentSetting;
            if (current != null && current.UserInfo.IsUser)
                return new MessageContract() { IsSuccess = true, Message = "user success" };
            return new MessageContract() { IsSuccess = false, Message = "wrong called signalgo bug is here" };
        }

        public MessageContract GustAccess()
        {
            return new MessageContract() { IsSuccess = true, Message = "gust success" };
        }
    }

    public class TestServerModel : ITestServerModel
    {
        public string HelloWorld(string yourName)
        {
            OperationContext<TestSetting>.CurrentSetting = new TestSetting() { Name = yourName };
            return "hello: " + yourName;
        }

        public bool Logout(string yourName)
        {
            throw new NotImplementedException();
        }

        public string WhoAmI()
        {
            return "you are : " + OperationContext<TestSetting>.CurrentSetting.Name;
        }

        public int MUL(int x, int y)
        {
            return x * y;
        }

        public double Tagh(double x, double y)
        {
            return x / y;
        }

        public TimeSpan TimeS(int x)
        {
            return new TimeSpan(x);
        }

        public long LongValue()
        {
            return long.MaxValue;
        }

        Tuple<bool> ITestServerModelBase.Logout(string yourName)
        {
            throw new NotImplementedException();
        }

        public List<UserInfoTest> GetListOfUsers()
        {
            List<UserInfoTest> results = new List<UserInfoTest>();
            UserInfoTest userInfoTest1 = new UserInfoTest() { Id = 1, Age = 25, Password = "123", Username = "aliUser" };
            userInfoTest1.PostInfoes = new List<PostInfoTest>() { new PostInfoTest() { Id = 1, PostSecurityLink = "securityLink", Text = "hello guys...", Title = "work finished", User = userInfoTest1 } };
            userInfoTest1.LastPostInfo = userInfoTest1.PostInfoes.FirstOrDefault();
            userInfoTest1.RoleInfoes = new List<RoleInfoTest>() { new RoleInfoTest() { Id = 1, Type = RoleTypeTest.Normal, User = userInfoTest1 } };
            results.Add(userInfoTest1);

            UserInfoTest userInfoTest2 = new UserInfoTest() { Id = 2, Age = 32, Password = "hello?123", Username = "rezaUser" };

            userInfoTest2.PostInfoes = new List<PostInfoTest>() { new PostInfoTest() { Id = 2, PostSecurityLink = "securityLink2", Text = "today were good but...", Title = "good day", User = userInfoTest2 } };
            userInfoTest2.LastPostInfo = userInfoTest2.PostInfoes.FirstOrDefault();
            userInfoTest2.RoleInfoes = new List<RoleInfoTest>() { new RoleInfoTest() { Id = 2, Type = RoleTypeTest.Admin, User = userInfoTest2 } };
            results.Add(userInfoTest2);

            return results;
        }

        public List<PostInfoTest> GetPostsOfUser(int userId)
        {
            List<PostInfoTest> results = new List<PostInfoTest>();
            results.Add(new PostInfoTest() { Id = 1, PostSecurityLink = "securityLink", Text = "hello guys...", Title = "work finished" });
            results.Add(new PostInfoTest() { Id = 2, PostSecurityLink = "securityLink2", Text = "today were good but...", Title = "good day" });
            return results;
        }

        public List<UserInfoTest> GetListOfUsersCustom()
        {
            return GetListOfUsers();
        }

        [CustomDataExchanger(typeof(PostInfoTest), "PostSecurityLink", "PostRoleToSee", ExchangeType = CustomDataExchangerType.TakeOnly, LimitationMode = LimitExchangeType.Both)]
        public List<PostInfoTest> GetCustomPostsOfUser(int userId)
        {
            List<PostInfoTest> results = new List<PostInfoTest>();
            results.Add(new PostInfoTest() { Id = 1, PostSecurityLink = "securityLink", Text = "hello guys...", Title = "work finished" });
            results.Add(new PostInfoTest() { Id = 2, PostSecurityLink = "securityLink2", Text = "today were good but...", Title = "good day" });
            results.Add(new PostInfoTest() { Id = 3, PostSecurityLink = "securityLink3", Text = "today were bad but...", Title = "bad day" });
            results.Add(new PostInfoTest() { Id = 4, PostSecurityLink = "securityLink4", Text = "today were bad but...", Title = "bad day" });
            results.Add(new PostInfoTest() { Id = 5, PostSecurityLink = "securityLink5", Text = "today were bad but...", Title = "bad day" });
            results.Add(new PostInfoTest() { Id = 6, PostSecurityLink = "securityLink6", Text = "today were bad but...", Title = "bad day" });
            results.Add(new PostInfoTest() { Id = 7, PostSecurityLink = "securityLink7", Text = "today were bad but...", Title = "bad day" });
            DataExchanger.Ignore(results[6]);
            DataExchanger.Ignore(results[5], "PostSecurityLink");
            DataExchanger.TakeOnly(results[4], "Id");
            return results;
        }

        [CustomDataExchanger(typeof(RoleInfoTest), "Id", ExchangeType = CustomDataExchangerType.TakeOnly, LimitationMode = LimitExchangeType.IncomingCall)]
        public bool Login(UserInfoTest userInfoTest)
        {
            if (userInfoTest.RoleInfoes == null || userInfoTest.PostInfoes != null || userInfoTest.LastPostInfo != null)
                return false;

            return userInfoTest.Username == "ali" && userInfoTest.Password == "123";
        }

        public bool HelloBind(UserInfoTest userInfoTest, UserInfoTest userInfoTest2, UserInfoTest userInfoTest3)
        {
            if (userInfoTest.Id == 15 && userInfoTest.Age == 0 && userInfoTest.Password == null && userInfoTest.Username == null && userInfoTest.RoleInfoes == null && userInfoTest.PostInfoes == null &&
                userInfoTest2.Id == 15 && userInfoTest2.Age == 10 && userInfoTest2.Username == null && userInfoTest2.Password != null && userInfoTest2.RoleInfoes[0].Id == 5 &&
                userInfoTest3.Id == 15 && userInfoTest3.Username == "user name" && userInfoTest3.Age == 0 && userInfoTest3.Password == null)
                return true;
            return false;
        }

        public async Task<string> ServerAsyncMethod(string name)
        {
            await Task.Delay(1500);
            if (name == "hello")
                return "hello guys";
            else
                return "not found";
        }

        public ArticleInfo AddArticle(ArticleInfo articleInfo)
        {
            return articleInfo;
        }

        public MessageContract<ArticleInfo> AddArticleMessage(ArticleInfo articleInfo)
        {
            return new MessageContract<ArticleInfo>() { IsSuccess = true, Data = articleInfo };
        }
    }
}
