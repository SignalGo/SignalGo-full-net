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

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestServerModel : ITestServerModelBase
    {
        //string HelloWorld([Bind(Exclude = "CategoryDescription")]string yourName);
        string HelloWorld(string yourName);
        List<UserInfoTest> GetListOfUsers();
        List<PostInfoTest> GetPostsOfUser(int userId);
        [CustomDataExchanger(typeof(UserInfoTest), "Id", "Password", "PostInfoes", ExchangeType = CustomDataExchangerType.Take, LimitationMode = LimitExchangeType.Both)]
        List<UserInfoTest> GetListOfUsersCustom();
        List<PostInfoTest> GetCustomPostsOfUser(int userId);
        bool Login(UserInfoTest userInfoTest);
    }

    [SignalGo.Shared.DataTypes.ServiceContract("TestServerModel", SignalGo.Shared.DataTypes.InstanceType.SingleInstance)]
    public interface ITestClientServerModel
    {
        string HelloWorld(string yourName);
        Task<string> HelloWorldAsync(string yourName);
    }
}
