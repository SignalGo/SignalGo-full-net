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
        System.Tuple<bool> Logout(string yourName);
        Task<System.Tuple<bool>> LogoutAsync(string yourName);
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
}

namespace SignalGoTest2Services.HttpServices
{
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

