using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.Publisher.Models
{
    public class SettingInfo
    {
        const string PublisherDbName = "PublisherData.json";
        const string ServersDbName = "ServersData.json";

        private static SettingInfo _Current = null;
        private static SettingInfo _CurrentServer = null;

        public static SettingInfo Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = LoadSettingInfo();
                }
                return _Current;
            }
        }
        public static SettingInfo CurrentServer
        {
            get
            {
                if (_CurrentServer == null)
                {
                    _CurrentServer = LoadServersSettingInfo();
                }
                return _CurrentServer;
            }
        }
        public Guid ProjectKey { get; set; }
        public ObservableCollection<ProjectInfo> ProjectInfo { get; set; } = new ObservableCollection<ProjectInfo>();

        public Guid ServerKey { get; set; }
        public ObservableCollection<ServerInfo> ServerInfo { get; set; } = new ObservableCollection<ServerInfo>();

        public static SettingInfo LoadSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PublisherData.json");
                if (!File.Exists(path))
                    return new SettingInfo()
                    {
                        ProjectInfo = new ObservableCollection<ProjectInfo>()
                    };
                return Newtonsoft.Json.JsonConvert.DeserializeObject<SettingInfo>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch
            {
                return new SettingInfo()
                {
                    ProjectInfo = new ObservableCollection<ProjectInfo>()
                };
            }
        }

        public static SettingInfo LoadServersSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServersData.json");
                if (!File.Exists(path))
                    return new SettingInfo()
                    {
                        ServerInfo = new ObservableCollection<ServerInfo>()
                    };
                return Newtonsoft.Json.JsonConvert.DeserializeObject<SettingInfo>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch
            {
                return new SettingInfo()
                {
                    ServerInfo = new ObservableCollection<ServerInfo>()
                };
            }
        }

        public static void SaveServersSettingInfo()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServersDbName);
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(CurrentServer), Encoding.UTF8);
        }
        public static void SaveSettingInfo()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherDbName);
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(Current), Encoding.UTF8);
        }
    }
}
