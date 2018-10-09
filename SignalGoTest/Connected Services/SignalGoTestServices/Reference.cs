using SignalGo.Shared.DataTypes;
using System.Threading.Tasks;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using SignalGoTest2Services.ServerServices;
using SignalGoTest2Services.HttpServices;
using SignalGoTest2Services.ClientServices;

namespace SignalGoTest2Services.ServerServices
{
    [ServiceContract("testservermodelserverservice",ServiceType.ServerService, InstanceType.SingleInstance)]
    public interface ITestServerModel
    {
        string HelloWorld(string yourName);
        Task<string> HelloWorldAsync(string yourName);
        System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest> GetListOfUsers();
        Task<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>> GetListOfUsersAsync();
        System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> GetPostsOfUser(int userId);
        Task<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>> GetPostsOfUserAsync(int userId);
        System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest> GetListOfUsersCustom();
        Task<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>> GetListOfUsersCustomAsync();
        System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> GetCustomPostsOfUser(int userId);
        Task<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>> GetCustomPostsOfUserAsync(int userId);
        bool HelloBind(SignalGoTest2.Models.UserInfoTest userInfoTest, SignalGoTest2.Models.UserInfoTest userInfoTest2, SignalGoTest2.Models.UserInfoTest userInfoTest3);
        Task<bool> HelloBindAsync(SignalGoTest2.Models.UserInfoTest userInfoTest, SignalGoTest2.Models.UserInfoTest userInfoTest2, SignalGoTest2.Models.UserInfoTest userInfoTest3);
        bool Login(SignalGoTest2.Models.UserInfoTest userInfoTest);
        Task<bool> LoginAsync(SignalGoTest2.Models.UserInfoTest userInfoTest);
        string ServerAsyncMethod(string name);
        Task<string> ServerAsyncMethodAsync(string name);
        SignalGoTest2.Models.ArticleInfo AddArticle(SignalGoTest2.Models.ArticleInfo articleInfo);
        Task<SignalGoTest2.Models.ArticleInfo> AddArticleAsync(SignalGoTest2.Models.ArticleInfo articleInfo);
        SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo> AddArticleMessage(SignalGoTest2.Models.ArticleInfo articleInfo);
        Task<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>> AddArticleMessageAsync(SignalGoTest2.Models.ArticleInfo articleInfo);
        System.Tuple<bool> Logout(string yourName);
        Task<System.Tuple<bool>> LogoutAsync(string yourName);
    }
}

namespace SignalGoTest2Services.StreamServices
{
    [ServiceContract("testserverstreammodelstreamservice",ServiceType.StreamService, InstanceType.SingleInstance)]
    public interface ITestServerStreamModel
    {
        SignalGo.Shared.Models.StreamInfo<string> DownloadImage(string name, SignalGoTest2.Models.TestStreamModel testStreamModel);
        Task<SignalGo.Shared.Models.StreamInfo<string>> DownloadImageAsync(string name, SignalGoTest2.Models.TestStreamModel testStreamModel);
    }
}

namespace SignalGoTest2Services.OneWayServices
{
    [ServiceContract("testservermodelonewayservice",ServiceType.OneWayService, InstanceType.SingleInstance)]
    public class TestServerModel
    {
        public static TestServerModel Current { get; set; }
        string _signalGoServerAddress = "";
        int _signalGoPortNumber = 0;
        public TestServerModel(string signalGoServerAddress, int signalGoPortNumber)
        {
            _signalGoServerAddress = signalGoServerAddress;
            _signalGoPortNumber = signalGoPortNumber;
        }
         public string HelloWorld(string yourName)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<string>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "HelloWorld", new SignalGo.Shared.Models.ParameterInfo() {  Name = "yourName", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) });
        }
         public Task<string> HelloWorldAsync(string yourName)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<string>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "HelloWorld", new SignalGo.Shared.Models.ParameterInfo() {  Name = "yourName", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) });
        }
         public System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest> GetListOfUsers()
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetListOfUsers");
        }
         public Task<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>> GetListOfUsersAsync()
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetListOfUsers");
        }
         public System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> GetPostsOfUser(int userId)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetPostsOfUser", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userId", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) });
        }
         public Task<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>> GetPostsOfUserAsync(int userId)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetPostsOfUser", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userId", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) });
        }
         public System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest> GetListOfUsersCustom()
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetListOfUsersCustom");
        }
         public Task<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>> GetListOfUsersCustomAsync()
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetListOfUsersCustom");
        }
         public System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> GetCustomPostsOfUser(int userId)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetCustomPostsOfUser", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userId", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) });
        }
         public Task<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>> GetCustomPostsOfUserAsync(int userId)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "GetCustomPostsOfUser", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userId", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) });
        }
         public bool HelloBind(SignalGoTest2.Models.UserInfoTest userInfoTest, SignalGoTest2.Models.UserInfoTest userInfoTest2, SignalGoTest2.Models.UserInfoTest userInfoTest3)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<bool>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "HelloBind", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) }, new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest2", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest2) }, new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest3", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest3) });
        }
         public Task<bool> HelloBindAsync(SignalGoTest2.Models.UserInfoTest userInfoTest, SignalGoTest2.Models.UserInfoTest userInfoTest2, SignalGoTest2.Models.UserInfoTest userInfoTest3)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<bool>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "HelloBind", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) }, new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest2", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest2) }, new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest3", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest3) });
        }
         public bool Login(SignalGoTest2.Models.UserInfoTest userInfoTest)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<bool>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "Login", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) });
        }
         public Task<bool> LoginAsync(SignalGoTest2.Models.UserInfoTest userInfoTest)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<bool>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "Login", new SignalGo.Shared.Models.ParameterInfo() {  Name = "userInfoTest", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) });
        }
         public string ServerAsyncMethod(string name)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<string>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "ServerAsyncMethod", new SignalGo.Shared.Models.ParameterInfo() {  Name = "name", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(name) });
        }
         public Task<string> ServerAsyncMethodAsync(string name)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<string>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "ServerAsyncMethod", new SignalGo.Shared.Models.ParameterInfo() {  Name = "name", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(name) });
        }
         public SignalGoTest2.Models.ArticleInfo AddArticle(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<SignalGoTest2.Models.ArticleInfo>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "AddArticle", new SignalGo.Shared.Models.ParameterInfo() {  Name = "articleInfo", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) });
        }
         public Task<SignalGoTest2.Models.ArticleInfo> AddArticleAsync(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<SignalGoTest2.Models.ArticleInfo>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "AddArticle", new SignalGo.Shared.Models.ParameterInfo() {  Name = "articleInfo", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) });
        }
         public SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo> AddArticleMessage(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "AddArticleMessage", new SignalGo.Shared.Models.ParameterInfo() {  Name = "articleInfo", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) });
        }
         public Task<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>> AddArticleMessageAsync(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "AddArticleMessage", new SignalGo.Shared.Models.ParameterInfo() {  Name = "articleInfo", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) });
        }
         public System.Tuple<bool> Logout(string yourName)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethod<System.Tuple<bool>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "Logout", new SignalGo.Shared.Models.ParameterInfo() {  Name = "yourName", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) });
        }
         public Task<System.Tuple<bool>> LogoutAsync(string yourName)
        {
                return SignalGo.Client.ClientProvider.SendOneWayMethodAsync<System.Tuple<bool>>(_signalGoServerAddress, _signalGoPortNumber, "testservermodelonewayservice", "Logout", new SignalGo.Shared.Models.ParameterInfo() {  Name = "yourName", Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) });
        }
    }
}

namespace SignalGoTest2Services.HttpServices
{
    public class TestServerModel
    {
        public TestServerModel(string serverUrl, SignalGo.Client.HttpClient httpClient = null)
        {
            _serverUrl = serverUrl;
            _httpClient = httpClient;
            if (_httpClient == null)
                _httpClient = new SignalGo.Client.HttpClient();
        }

        private readonly string _serverUrl = null;
        private SignalGo.Client.HttpClient _httpClient;
        public SignalGo.Shared.Http.WebHeaderCollection RequestHeaders
        {
            get
            {
                return _httpClient.RequestHeaders;
            }
            set
            {
                _httpClient.RequestHeaders = value;
            }
        }

        public SignalGo.Shared.Http.WebHeaderCollection ResponseHeaders { get; set; }
        public System.Net.HttpStatusCode Status { get; set; }
        public string HelloWorld(string yourName)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/HelloWorld", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(yourName),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<string>(result.Data);
        }
        public async Task<string> HelloWorldAsync(string yourName)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/HelloWorld", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(yourName),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<string>(result.Data);
        }
        public System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest> GetListOfUsers()
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetListOfUsers", new SignalGo.Shared.Models.ParameterInfo[]
                {
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(result.Data);
        }
        public async Task<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>> GetListOfUsersAsync()
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetListOfUsers", new SignalGo.Shared.Models.ParameterInfo[]
                {
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(result.Data);
        }
        public System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> GetPostsOfUser(int userId)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetPostsOfUser", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userId),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(result.Data);
        }
        public async Task<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>> GetPostsOfUserAsync(int userId)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetPostsOfUser", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userId),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(result.Data);
        }
        public System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest> GetListOfUsersCustom()
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetListOfUsersCustom", new SignalGo.Shared.Models.ParameterInfo[]
                {
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(result.Data);
        }
        public async Task<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>> GetListOfUsersCustomAsync()
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetListOfUsersCustom", new SignalGo.Shared.Models.ParameterInfo[]
                {
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.UserInfoTest>>(result.Data);
        }
        public System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> GetCustomPostsOfUser(int userId)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetCustomPostsOfUser", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userId),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(result.Data);
        }
        public async Task<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>> GetCustomPostsOfUserAsync(int userId)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/GetCustomPostsOfUser", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userId),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userId) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest>>(result.Data);
        }
        public bool HelloBind(SignalGoTest2.Models.UserInfoTest userInfoTest, SignalGoTest2.Models.UserInfoTest userInfoTest2, SignalGoTest2.Models.UserInfoTest userInfoTest3)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/HelloBind", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) },
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest2),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest2) },
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest3),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest3) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<bool>(result.Data);
        }
        public async Task<bool> HelloBindAsync(SignalGoTest2.Models.UserInfoTest userInfoTest, SignalGoTest2.Models.UserInfoTest userInfoTest2, SignalGoTest2.Models.UserInfoTest userInfoTest3)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/HelloBind", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) },
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest2),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest2) },
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest3),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest3) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<bool>(result.Data);
        }
        public bool Login(SignalGoTest2.Models.UserInfoTest userInfoTest)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/Login", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<bool>(result.Data);
        }
        public async Task<bool> LoginAsync(SignalGoTest2.Models.UserInfoTest userInfoTest)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/Login", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(userInfoTest),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(userInfoTest) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<bool>(result.Data);
        }
        public string ServerAsyncMethod(string name)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/ServerAsyncMethod", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(name),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(name) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<string>(result.Data);
        }
        public async Task<string> ServerAsyncMethodAsync(string name)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/ServerAsyncMethod", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(name),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(name) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<string>(result.Data);
        }
        public SignalGoTest2.Models.ArticleInfo AddArticle(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/AddArticle", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(articleInfo),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<SignalGoTest2.Models.ArticleInfo>(result.Data);
        }
        public async Task<SignalGoTest2.Models.ArticleInfo> AddArticleAsync(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/AddArticle", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(articleInfo),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<SignalGoTest2.Models.ArticleInfo>(result.Data);
        }
        public SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo> AddArticleMessage(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/AddArticleMessage", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(articleInfo),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>>(result.Data);
        }
        public async Task<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>> AddArticleMessageAsync(SignalGoTest2.Models.ArticleInfo articleInfo)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/AddArticleMessage", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(articleInfo),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(articleInfo) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<SignalGoTest2.Models.MessageContract<SignalGoTest2.Models.ArticleInfo>>(result.Data);
        }
        public System.Tuple<bool> Logout(string yourName)
        {
                SignalGo.Client.HttpClientResponse result = _httpClient.Post(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/Logout", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(yourName),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Tuple<bool>>(result.Data);
        }
        public async Task<System.Tuple<bool>> LogoutAsync(string yourName)
        {
                SignalGo.Client.HttpClientResponse result = await _httpClient.PostAsync(_serverUrl + (_serverUrl.EndsWith("/") ? "" : "/") + "testservermodel/Logout", new SignalGo.Shared.Models.ParameterInfo[]
                {
                         new  SignalGo.Shared.Models.ParameterInfo() { Name = nameof(yourName),Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(yourName) },
                });
                ResponseHeaders = result.ResponseHeaders;
                Status = result.Status;
                return SignalGo.Client.ClientSerializationHelper.DeserializeObject<System.Tuple<bool>>(result.Data);
        }
    }
}

namespace SignalGoTest2.Models
{
    public class TestStreamModel : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private string _Name;
        public string Name
        {
                get
                {
                        return _Name;
                }
                set
                {
                        _Name = value;
                        OnPropertyChanged(nameof(Name));
                }
        }

        private System.Collections.Generic.List<string> _Values;
        public System.Collections.Generic.List<string> Values
        {
                get
                {
                        return _Values;
                }
                set
                {
                        _Values = value;
                        OnPropertyChanged(nameof(Values));
                }
        }


    }

    public class UserInfoTest : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private int _Id;
        public int Id
        {
                get
                {
                        return _Id;
                }
                set
                {
                        _Id = value;
                        OnPropertyChanged(nameof(Id));
                }
        }

        private string _Username;
        public string Username
        {
                get
                {
                        return _Username;
                }
                set
                {
                        _Username = value;
                        OnPropertyChanged(nameof(Username));
                }
        }

        private string _Password;
        public string Password
        {
                get
                {
                        return _Password;
                }
                set
                {
                        _Password = value;
                        OnPropertyChanged(nameof(Password));
                }
        }

        private int _Age;
        public int Age
        {
                get
                {
                        return _Age;
                }
                set
                {
                        _Age = value;
                        OnPropertyChanged(nameof(Age));
                }
        }

        private System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> _PostInfoes;
        public System.Collections.Generic.List<SignalGoTest2.Models.PostInfoTest> PostInfoes
        {
                get
                {
                        return _PostInfoes;
                }
                set
                {
                        _PostInfoes = value;
                        OnPropertyChanged(nameof(PostInfoes));
                }
        }

        private System.Collections.Generic.List<SignalGoTest2.Models.RoleInfoTest> _RoleInfoes;
        public System.Collections.Generic.List<SignalGoTest2.Models.RoleInfoTest> RoleInfoes
        {
                get
                {
                        return _RoleInfoes;
                }
                set
                {
                        _RoleInfoes = value;
                        OnPropertyChanged(nameof(RoleInfoes));
                }
        }

        private SignalGoTest2.Models.PostInfoTest _LastPostInfo;
        public SignalGoTest2.Models.PostInfoTest LastPostInfo
        {
                get
                {
                        return _LastPostInfo;
                }
                set
                {
                        _LastPostInfo = value;
                        OnPropertyChanged(nameof(LastPostInfo));
                }
        }


    }

    public class PostInfoTest : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private int _Id;
        public int Id
        {
                get
                {
                        return _Id;
                }
                set
                {
                        _Id = value;
                        OnPropertyChanged(nameof(Id));
                }
        }

        private string _Title;
        public string Title
        {
                get
                {
                        return _Title;
                }
                set
                {
                        _Title = value;
                        OnPropertyChanged(nameof(Title));
                }
        }

        private string _Text;
        public string Text
        {
                get
                {
                        return _Text;
                }
                set
                {
                        _Text = value;
                        OnPropertyChanged(nameof(Text));
                }
        }

        private string _PostSecurityLink;
        public string PostSecurityLink
        {
                get
                {
                        return _PostSecurityLink;
                }
                set
                {
                        _PostSecurityLink = value;
                        OnPropertyChanged(nameof(PostSecurityLink));
                }
        }

        private SignalGoTest2.Models.UserInfoTest _User;
        public SignalGoTest2.Models.UserInfoTest User
        {
                get
                {
                        return _User;
                }
                set
                {
                        _User = value;
                        OnPropertyChanged(nameof(User));
                }
        }

        private SignalGoTest2.Models.RoleInfoTest _PostRoleToSee;
        public SignalGoTest2.Models.RoleInfoTest PostRoleToSee
        {
                get
                {
                        return _PostRoleToSee;
                }
                set
                {
                        _PostRoleToSee = value;
                        OnPropertyChanged(nameof(PostRoleToSee));
                }
        }


    }

    public class ArticleInfo : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private string _Name;
        public string Name
        {
                get
                {
                        return _Name;
                }
                set
                {
                        _Name = value;
                        OnPropertyChanged(nameof(Name));
                }
        }

        private string _Detail;
        public string Detail
        {
                get
                {
                        return _Detail;
                }
                set
                {
                        _Detail = value;
                        OnPropertyChanged(nameof(Detail));
                }
        }

        private System.DateTime? _CreatedDateTime;
        public System.DateTime? CreatedDateTime
        {
                get
                {
                        return _CreatedDateTime;
                }
                set
                {
                        _CreatedDateTime = value;
                        OnPropertyChanged(nameof(CreatedDateTime));
                }
        }


    }

    public class MessageContract<T> : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private T _Data;
        public T Data
        {
                get
                {
                        return _Data;
                }
                set
                {
                        _Data = value;
                        OnPropertyChanged(nameof(Data));
                }
        }

        private bool _IsSuccess;
        public bool IsSuccess
        {
                get
                {
                        return _IsSuccess;
                }
                set
                {
                        _IsSuccess = value;
                        OnPropertyChanged(nameof(IsSuccess));
                }
        }

        private System.Collections.Generic.List<SignalGoTest2.Models.ValidationRule> _Errors;
        public System.Collections.Generic.List<SignalGoTest2.Models.ValidationRule> Errors
        {
                get
                {
                        return _Errors;
                }
                set
                {
                        _Errors = value;
                        OnPropertyChanged(nameof(Errors));
                }
        }


    }

    public class RoleInfoTest : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private int _Id;
        public int Id
        {
                get
                {
                        return _Id;
                }
                set
                {
                        _Id = value;
                        OnPropertyChanged(nameof(Id));
                }
        }

        private SignalGoTest2.Models.RoleTypeTest _Type;
        public SignalGoTest2.Models.RoleTypeTest Type
        {
                get
                {
                        return _Type;
                }
                set
                {
                        _Type = value;
                        OnPropertyChanged(nameof(Type));
                }
        }

        private SignalGoTest2.Models.UserInfoTest _User;
        public SignalGoTest2.Models.UserInfoTest User
        {
                get
                {
                        return _User;
                }
                set
                {
                        _User = value;
                        OnPropertyChanged(nameof(User));
                }
        }


    }

    public class ValidationRule : SignalGo.Shared.Models.NotifyPropertyChangedBase
    {
        private string _Name;
        public string Name
        {
                get
                {
                        return _Name;
                }
                set
                {
                        _Name = value;
                        OnPropertyChanged(nameof(Name));
                }
        }

        private string _Message;
        public string Message
        {
                get
                {
                        return _Message;
                }
                set
                {
                        _Message = value;
                        OnPropertyChanged(nameof(Message));
                }
        }


    }

}

namespace SignalGoTest2Services.ClientServices
{
}

namespace SignalGoTest2.Models
{
    public enum RoleTypeTest : int
    {
        Normal = 0,
        Admin = 1,
        Editor = 2,
        Viewer = 3,
    }

}

