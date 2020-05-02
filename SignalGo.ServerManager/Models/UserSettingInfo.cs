using System;
using System.IO;
using System.Text;

namespace SignalGo.ServerManager.Models
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
                    SaveUserSettingInfo();
                }
                return _Current;
            }
        }
        public UserSetting UserSettings { get; set; } = new UserSetting();

        public static UserSettingInfo LoadUserSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName);
                if (!File.Exists(path) || File.ReadAllLinesAsync(path).Result.Length <= 0)
                {
                    return new UserSettingInfo()
                    {
                        UserSettings = new UserSetting
                        {
                            BackupPath = "C:\\ServerManagerBackups",
                            ListeningPort = "6464",
                            ListeningAddress = "localhost",
                            LoggerPath = "AppLogs.log"
                        }
                    };
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<UserSettingInfo>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), Encoding.UTF8));
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
