using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.ServerManager.Models
{
    public class SettingInfo
    {
        private static SettingInfo _Current = null;

        private readonly static string ServerDbName = "Data.json";

        public static SettingInfo Current
        {
            get
            {
                if (_Current == null)
                    _Current = LoadSettingInfo();
                return _Current;
            }
        }
        public Guid ServerKey { get; set; }
        public ObservableCollection<ServerInfo> ServerInfo { get; set; } = new ObservableCollection<ServerInfo>();

        public static SettingInfo LoadSettingInfo()
        {
            try
            {
                if (!File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServerDbName)))
                    return new SettingInfo()
                    {
                        ServerInfo = new ObservableCollection<ServerInfo>()
                    };
                return Newtonsoft.Json.JsonConvert.DeserializeObject<SettingInfo>(File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServerDbName), Encoding.UTF8));
            }
            catch
            {
                return new SettingInfo()
                {
                    ServerInfo = new ObservableCollection<ServerInfo>()
                };
            }
        }

        public static void SaveSettingInfo()
        {
            File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServerDbName), Newtonsoft.Json.JsonConvert.SerializeObject(Current), Encoding.UTF8);
        }
    }
}
