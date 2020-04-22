using System;
using System.IO;
using System.Text;

namespace SignalGo.Publisher.Models
{
    public class UserSettingInfo
    {
        private static UserSettingInfo _Current = null;

        private readonly static string UserSettingsDbName = "UserData.json";

        public static UserSettingInfo Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = LoadUserSettingInfo();
                }
                return _Current;
            }
        }
        public UserSetting UserSettings { get; set; } = new UserSetting();

        public static UserSettingInfo LoadUserSettingInfo()
        {
            try
            {
                if (!File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName)))
                    return new UserSettingInfo()
                    {
                        UserSettings = new UserSetting()
                    };
                return Newtonsoft.Json.JsonConvert.DeserializeObject<UserSettingInfo>(File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), Encoding.UTF8));
            }
            catch
            {
                return new UserSettingInfo()
                {
                    UserSettings = new UserSetting()
                };
            }
        }

        public static void SaveUserSettingInfo()
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), Newtonsoft.Json.JsonConvert.SerializeObject(Current), Encoding.UTF8);
        }
    }
}
