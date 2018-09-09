using SignalGo.Shared.DataTypes;
using System.Threading.Tasks;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using SignalGoTestServices.ServerServices;
using SignalGoTestServices.HttpServices;
using SignalGoTestServices.ClientServices;

namespace SignalGoTestServices.ServerServices
{
    [ServiceContract("testservermodelserverservice",ServiceType.ServerService, InstanceType.SingleInstance)]
    public interface ITestServerModel
    {
        System.Tuple<bool> Logout(string yourName);
        Task<System.Tuple<bool>> LogoutAsync(string yourName);
        string HelloWorld(string yourName);
        Task<string> HelloWorldAsync(string yourName);
        System.Collections.Generic.List<SignalGoTest.ClientModels.UserInfoTest> GetListOfUsers();
        Task<System.Collections.Generic.List<SignalGoTest.ClientModels.UserInfoTest>> GetListOfUsersAsync();
        System.Collections.Generic.List<SignalGoTest.ClientModels.PostInfoTest> GetPostsOfUser(int userId);
        Task<System.Collections.Generic.List<SignalGoTest.ClientModels.PostInfoTest>> GetPostsOfUserAsync(int userId);
        System.Collections.Generic.List<SignalGoTest.ClientModels.UserInfoTest> GetListOfUsersCustom();
        Task<System.Collections.Generic.List<SignalGoTest.ClientModels.UserInfoTest>> GetListOfUsersCustomAsync();
        System.Collections.Generic.List<SignalGoTest.ClientModels.PostInfoTest> GetCustomPostsOfUser(int userId);
        Task<System.Collections.Generic.List<SignalGoTest.ClientModels.PostInfoTest>> GetCustomPostsOfUserAsync(int userId);
        bool HelloBind(SignalGoTest.ClientModels.UserInfoTest userInfoTest, SignalGoTest.ClientModels.UserInfoTest userInfoTest2, SignalGoTest.ClientModels.UserInfoTest userInfoTest3);
        Task<bool> HelloBindAsync(SignalGoTest.ClientModels.UserInfoTest userInfoTest, SignalGoTest.ClientModels.UserInfoTest userInfoTest2, SignalGoTest.ClientModels.UserInfoTest userInfoTest3);
        bool Login(SignalGoTest.ClientModels.UserInfoTest userInfoTest);
        Task<bool> LoginAsync(SignalGoTest.ClientModels.UserInfoTest userInfoTest);
    }
}

namespace SignalGoTestServices.StreamServices
{
    [ServiceContract("testserverstreammodelstreamservice",ServiceType.StreamService, InstanceType.SingleInstance)]
    public interface ITestServerStreamModel
    {
        SignalGo.Shared.Models.StreamInfo<string> DownloadImage(string name, SignalGoTest.ClientModels.TestStreamModel testStreamModel);
        Task<SignalGo.Shared.Models.StreamInfo<string>> DownloadImageAsync(string name, SignalGoTest.ClientModels.TestStreamModel testStreamModel);
    }
}

namespace SignalGoTestServices.OneWayServices
{
}

namespace SignalGoTestServices.HttpServices
{
}

namespace SignalGoTest.ClientModels
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

        private System.Collections.Generic.List<SignalGoTest.ClientModels.PostInfoTest> _PostInfoes;
        public System.Collections.Generic.List<SignalGoTest.ClientModels.PostInfoTest> PostInfoes
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

        private System.Collections.Generic.List<SignalGoTest.ClientModels.RoleInfoTest> _RoleInfoes;
        public System.Collections.Generic.List<SignalGoTest.ClientModels.RoleInfoTest> RoleInfoes
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

        private SignalGoTest.ClientModels.PostInfoTest _LastPostInfo;
        public SignalGoTest.ClientModels.PostInfoTest LastPostInfo
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

        private SignalGoTest.ClientModels.UserInfoTest _User;
        public SignalGoTest.ClientModels.UserInfoTest User
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

        private SignalGoTest.ClientModels.RoleInfoTest _PostRoleToSee;
        public SignalGoTest.ClientModels.RoleInfoTest PostRoleToSee
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

        private SignalGoTest.ClientModels.RoleTypeTest _Type;
        public SignalGoTest.ClientModels.RoleTypeTest Type
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

        private SignalGoTest.ClientModels.UserInfoTest _User;
        public SignalGoTest.ClientModels.UserInfoTest User
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

}

namespace SignalGoTestServices.ClientServices
{
}

namespace SignalGoTest.ClientModels
{
    public enum RoleTypeTest : int
    {
        Normal = 0,
        Admin = 1,
        Editor = 2,
        Viewer = 3,
    }

}

