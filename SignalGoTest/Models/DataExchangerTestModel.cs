using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class UserInfoTest
    {
        public int Id { get; set; }
        public string Username { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.OutgoingCall)]
        public string Password { get; set; }
        public int Age { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.IncomingCall)]
        public List<PostInfoTest> PostInfoes { get; set; }
        public List<RoleInfoTest> RoleInfoes { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.Both)]
        public PostInfoTest LastPostInfo { get; set; }
    }

    [CustomDataExchanger("Title", "Text", ExchangeType = CustomDataExchangerType.Take, LimitationMode = LimitExchangeType.IncomingCall)]
    [CustomDataExchanger("Id", "Title", "Text", ExchangeType = CustomDataExchangerType.Take, LimitationMode = LimitExchangeType.OutgoingCall)]
    public class PostInfoTest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string PostSecurityLink { get; set; }
        [CustomDataExchanger(ExchangeType = CustomDataExchangerType.Ignore, LimitationMode = LimitExchangeType.Both)]
        public UserInfoTest User { get; set; }
        public RoleInfoTest PostRoleToSee { get; set; }
    }

    public enum RoleTypeTest
    {
        Normal,
        Admin,
        Editor,
        Viewer
    }

    public class RoleInfoTest
    {
        public int Id { get; set; }
        public RoleTypeTest Type { get; set; }
        public UserInfoTest User { get; set; }
    }
}
