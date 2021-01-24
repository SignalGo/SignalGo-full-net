using SignalGoTest2.Models;
using SignalGoTest2Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SignalGoTest.DataExhanger
{
    public class DataExchangerTest
    {

        private Func<T, T> CreateNewStatement<T>(params string[] fields)
        {
            // input parameter "o"
            ParameterExpression xParameter = Expression.Parameter(typeof(T), "o");

            // new statement "new Data()"
            NewExpression xNew = Expression.New(typeof(T));

            // create initializers
            IEnumerable<MemberAssignment> bindings = fields.Select(o => o.Trim())
                .Select(o =>
                {
                    // property "Field1"
                    System.Reflection.PropertyInfo mi = typeof(T).GetProperty(o);
                    // original value "o.Field1"
                    MemberExpression xOriginal = Expression.Property(xParameter, mi);
                    // set value "Field1 = o.Field1"
                    return Expression.Bind(mi, xOriginal);
                }
            );

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            MemberInitExpression xInit = Expression.MemberInit(xNew, bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            Expression<Func<T, T>> lambda = Expression.Lambda<Func<T, T>>(xInit, xParameter);

            // compile to Func<Data, Data>
            return lambda.Compile();
        }

        [Fact]
        public void TestDataExchanger()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            //while (true)
            //    System.Threading.Thread.Sleep(100);
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = service.HelloBind(test, test1, test2);
            Assert.True(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = service.GetListOfUsers();
            Assert.True(string.IsNullOrEmpty(users[0].Password));
            Assert.True(users[0].LastPostInfo == null);
            Assert.True(users[0].PostInfoes != null);
            Assert.True(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.True(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = service.GetPostsOfUser(1);
            Assert.True(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.True(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = service.GetCustomPostsOfUser(1);
            Assert.True(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.True(customPosts[0].User == null);
            Assert.True(customPosts.Count == 6);
            Assert.True(customPosts[5].PostSecurityLink == null);
            Assert.True(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.True(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = service.GetListOfUsersCustom();
            Assert.True(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.True(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.True(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.True(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.True(loginResult);
        }

        [Fact]
        public async Task TestDataExchangerAsync()
        {
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();

            ITestServerModel service = client.RegisterServerServiceInterfaceWrapper<ITestServerModel>();
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = await service.HelloBindAsync(test, test1, test2);
            Assert.True(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = await service.GetListOfUsersAsync();
            Assert.True(string.IsNullOrEmpty(users[0].Password));
            Assert.True(users[0].LastPostInfo == null);
            Assert.True(users[0].PostInfoes != null);
            Assert.True(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.True(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = await service.GetPostsOfUserAsync(1);
            Assert.True(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.True(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = await service.GetCustomPostsOfUserAsync(1);
            Assert.True(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.True(customPosts[0].User == null);
            Assert.True(customPosts.Count == 6);
            Assert.True(customPosts[5].PostSecurityLink == null);
            Assert.True(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.True(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = await service.GetListOfUsersCustomAsync();
            Assert.True(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.True(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.True(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.True(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = await service.LoginAsync(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.True(loginResult);
        }

        [Fact]
        public void TestDataExchangerOneWay()
        {
            SignalGoTest2Services.OneWayServices.TestServerModel service = new SignalGoTest2Services.OneWayServices.TestServerModel("localhost", 1132);
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = service.HelloBind(test, test1, test2);
            Assert.True(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = service.GetListOfUsers();
            Assert.True(string.IsNullOrEmpty(users[0].Password));
            Assert.True(users[0].LastPostInfo == null);
            Assert.True(users[0].PostInfoes != null);
            Assert.True(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.True(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = service.GetPostsOfUser(1);
            Assert.True(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.True(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = service.GetCustomPostsOfUser(1);
            Assert.True(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.True(customPosts[0].User == null);
            Assert.True(customPosts.Count == 6);
            Assert.True(customPosts[5].PostSecurityLink == null);
            Assert.True(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.True(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = service.GetListOfUsersCustom();
            Assert.True(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.True(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.True(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.True(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.True(loginResult);
        }

        [Fact]
        public async Task TestDataExchangerOneWayAsync()
        {
            SignalGoTest2Services.OneWayServices.TestServerModel service = new SignalGoTest2Services.OneWayServices.TestServerModel("localhost", 1132);

            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = await service.HelloBindAsync(test, test1, test2);
            Assert.True(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = await service.GetListOfUsersAsync();
            Assert.True(string.IsNullOrEmpty(users[0].Password));
            Assert.True(users[0].LastPostInfo == null);
            Assert.True(users[0].PostInfoes != null);
            Assert.True(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.True(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = await service.GetPostsOfUserAsync(1);
            Assert.True(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.True(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = await service.GetCustomPostsOfUserAsync(1);
            Assert.True(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.True(customPosts[0].User == null);
            Assert.True(customPosts.Count == 6);
            Assert.True(customPosts[5].PostSecurityLink == null);
            Assert.True(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.True(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = await service.GetListOfUsersCustomAsync();
            Assert.True(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.True(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.True(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.True(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = await service.LoginAsync(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.True(loginResult);
        }


        [Fact]
        public void TestDataExchangerHttp()
        {
            SignalGoTest2Services.HttpServices.TestServerModel service = new SignalGoTest2Services.HttpServices.TestServerModel("http://localhost:1132");
            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = service.HelloBind(test, test1, test2);
            Assert.True(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = service.GetListOfUsers();
            Assert.True(string.IsNullOrEmpty(users[0].Password));
            Assert.True(users[0].LastPostInfo == null);
            Assert.True(users[0].PostInfoes != null);
            Assert.True(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.True(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = service.GetPostsOfUser(1);
            Assert.True(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.True(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = service.GetCustomPostsOfUser(1);
            Assert.True(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.True(customPosts[0].User == null);
            Assert.True(customPosts.Count == 6);
            Assert.True(customPosts[5].PostSecurityLink == null);
            Assert.True(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.True(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = service.GetListOfUsersCustom();
            Assert.True(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.True(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.True(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.True(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = service.Login(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.True(loginResult);
        }

        [Fact]
        public async Task TestDataExchangerHttpAsync()
        {
            SignalGoTest2Services.HttpServices.TestServerModel service = new SignalGoTest2Services.HttpServices.TestServerModel("http://localhost:1132");

            UserInfoTest test = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };
            UserInfoTest test1 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Id = 5 } } };
            UserInfoTest test2 = new UserInfoTest() { Age = 10, Id = 15, Username = "user name", LastPostInfo = new PostInfoTest() { }, Password = "pass", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { } } };

            bool helloBind = await service.HelloBindAsync(test, test1, test2);
            Assert.True(helloBind);
            System.Collections.Generic.List<UserInfoTest> users = await service.GetListOfUsersAsync();
            Assert.True(string.IsNullOrEmpty(users[0].Password));
            Assert.True(users[0].LastPostInfo == null);
            Assert.True(users[0].PostInfoes != null);
            Assert.True(string.IsNullOrEmpty(users[0].PostInfoes[0].PostSecurityLink));
            Assert.True(users[0].PostInfoes[0].User == null);

            System.Collections.Generic.List<PostInfoTest> posts = await service.GetPostsOfUserAsync(1);
            Assert.True(string.IsNullOrEmpty(posts[0].PostSecurityLink));
            Assert.True(posts[0].User == null);

            System.Collections.Generic.List<PostInfoTest> customPosts = await service.GetCustomPostsOfUserAsync(1);
            Assert.True(!string.IsNullOrEmpty(customPosts[0].PostSecurityLink));
            Assert.True(customPosts[0].User == null);
            Assert.True(customPosts.Count == 6);
            Assert.True(customPosts[5].PostSecurityLink == null);
            Assert.True(customPosts[4].Id > 0 && customPosts[4].PostSecurityLink == null);
            Assert.True(string.IsNullOrEmpty(customPosts[0].Title) && string.IsNullOrEmpty(customPosts[1].Text) && customPosts[0].Id == customPosts[1].Id);

            System.Collections.Generic.List<UserInfoTest> customUsers = await service.GetListOfUsersCustomAsync();
            Assert.True(!string.IsNullOrEmpty(customUsers[0].Password));
            Assert.True(string.IsNullOrEmpty((customUsers[1].Username)));
            Assert.True(string.IsNullOrEmpty((customUsers[1].PostInfoes[0].PostSecurityLink)));
            Assert.True(!string.IsNullOrEmpty((customUsers[1].PostInfoes[0].Text)));
            bool loginResult = await service.LoginAsync(new UserInfoTest() { Id = 1, Age = 20, LastPostInfo = new PostInfoTest() { Text = "aa" }, Username = "ali", Password = "123", PostInfoes = new System.Collections.Generic.List<PostInfoTest>() { new PostInfoTest() { Text = "re" } }, RoleInfoes = new System.Collections.Generic.List<RoleInfoTest>() { new RoleInfoTest() { Type = RoleTypeTest.Admin, User = new UserInfoTest() { Username = "w" }, Id = 5 } } });
            Assert.True(loginResult);
        }
    }
}
