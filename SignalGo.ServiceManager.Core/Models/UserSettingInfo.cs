using Newtonsoft.Json;
using SignalGo.ServiceManager.Core.Engines.Models;
using SignalGo.ServiceManager.Core.Helpers;
using SignalGo.Shared.Log;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.ServiceManager.Core.Models
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
        public ObservableCollection<HealthCheckInfo> HealthChecks { get; set; } = new ObservableCollection<HealthCheckInfo>();
        public static UserSettingInfo LoadUserSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName);
                if (!File.Exists(path) || File.ReadAllLines(path).Length <= 0)
                {
                    File.Create(path).Dispose();
                    return new UserSettingInfo()
                    {
                        UserSettings = new UserSetting
                        {
                            DotNetPath = Utils.FindDotNetPath(),
                            BackupPath = Utils.FindBackupPath(),
                            ListeningPort = "6464",
                            ListeningAddress = "localhost",
                            LoggerPath = Path.Combine(Environment.CurrentDirectory, "AppLogs.log"),
                            ServiceUpdaterLogFilePath = Path.Combine(Environment.CurrentDirectory, "ServiceUpdateLogs.log")
                        }
                    };
                }
                var result = JsonConvert.DeserializeObject<UserSettingInfo>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), Encoding.UTF8));
                if (result == null)
                    throw new NullReferenceException();
                return result;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "LoadUserSettingInfo");
                return new UserSettingInfo()
                {
                    UserSettings = new UserSetting()
                };
            }
        }

        public static void SaveUserSettingInfo()
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), JsonConvert.SerializeObject(Current, Formatting.Indented), Encoding.UTF8);
        }
    }
}
