using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.ServerManager.Models
{
    public class SettingInfo
    {
        private static SettingInfo _Current = null;
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
        public ObservableCollection<ServerInfo> ServerInfoes { get; set; } = new ObservableCollection<ServerInfo>();

        public static SettingInfo LoadSettingInfo()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.db");
                if (!File.Exists(path))
                    return new SettingInfo()
                    {
                        ServerInfoes = new ObservableCollection<ServerInfo>()
                    };
                return Newtonsoft.Json.JsonConvert.DeserializeObject<SettingInfo>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch
            {
                return new SettingInfo()
                {
                    ServerInfoes = new ObservableCollection<ServerInfo>()
                };
            }
        }

        public static void SaveSettingInfo()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data.db");
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(Current), Encoding.UTF8);
        }
    }
}
