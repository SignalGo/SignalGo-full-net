using Newtonsoft.Json;
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
                            LoggerPath = "AppLogs.log",
                            MsbuildPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe",
                            CommandRunnerLogsPath = "CommandRunnerLogs.log",
                            ServiceUpdaterLogFilePath = "ServiceUpdaterLog.log",
                            DefaultTestRunner = UserSetting.TestRunnersEnum.NetCoreSDK,
                            TestRunnerExecutableFile = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow\\vstest.console.exe",
                            StartPriority = "Normal",
                            MaxThreads = 1,
                        }
                    };
                }
                return JsonConvert.DeserializeObject<UserSettingInfo>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), Encoding.UTF8));
            }
            catch (Exception ex)
            {
                ServerInfo.ServerLogs.Add(ex.Message + "\n");
                return new UserSettingInfo()
                {
                    //UserSettings = new UserSetting()
                };
            }
        }

        public static void SaveUserSettingInfo()
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UserSettingsDbName), JsonConvert.SerializeObject(Current, Formatting.Indented), Encoding.UTF8);
        }
    }
}
