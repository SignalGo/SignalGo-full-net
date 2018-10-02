using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public interface ITestServerModelBase
    {
        Tuple<bool> Logout(string yourName);
    }

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", ServiceType.ServerService, InstanceType = SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestServerModel : ITestServerModelBase
    {
        //string HelloWorld([Bind(Excludes = new string[] { "CategoryDescription" })]string yourName);
        string HelloWorld(string yourName);
        List<UserInfoTest> GetListOfUsers();
        List<PostInfoTest> GetPostsOfUser(int userId);
        [CustomDataExchanger(typeof(UserInfoTest), "Id", "Password", "PostInfoes", ExchangeType = CustomDataExchangerType.TakeOnly, LimitationMode = LimitExchangeType.OutgoingCall)]
        List<UserInfoTest> GetListOfUsersCustom();
        List<PostInfoTest> GetCustomPostsOfUser(int userId);
        bool HelloBind([Bind(Include = "Id")]UserInfoTest userInfoTest, [Bind(Exclude = "Username")]UserInfoTest userInfoTest2,
            [Bind(Includes = new string[] { "Id", "Username" })]UserInfoTest userInfoTest3);
        bool Login(UserInfoTest userInfoTest);
        Task<string> ServerAsyncMethod(string name);
    }

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", ServiceType.ClientService, InstanceType = SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestClientServiceModel
    {
        string HelloWorld(string yourName);
        Task<string> HelloWorldAsync(string yourName);
    }
}
