using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.Publisher.Models
{
    public class ServerSettingInfo
    {
        const string ServersDbName = "ServersData.json";
        private static ServerSettingInfo _CurrentServer = null;

        public static ServerSettingInfo CurrentServer
        {
            get
            {
                if (_CurrentServer == null)
                {
                    _CurrentServer = LoadServersSettingInfo();
                    SaveServersSettingInfo();
                }
                return _CurrentServer;
            }
        }
        [JsonIgnore]
        public Guid ServerKey { get; set; }
        public ObservableCollection<ServerInfo> ServerInfo { get; set; } = new ObservableCollection<ServerInfo>();

        public static ServerSettingInfo LoadServersSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServersDbName);
                if (!File.Exists(path) || File.ReadAllLinesAsync(path).Result.Length <= 0)
                {
                    File.Delete(path);
                    return new ServerSettingInfo()
                    {
                        ServerInfo = new ObservableCollection<ServerInfo>()
                    };
                }
                return JsonConvert.DeserializeObject<ServerSettingInfo>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                return new ServerSettingInfo()
                {
                    ServerInfo = new ObservableCollection<ServerInfo>()
                };
            }
        }

        public static void SaveServersSettingInfo()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServersDbName);
            File.WriteAllText(path, JsonConvert.SerializeObject(CurrentServer), Encoding.UTF8);
        }
    }
}
