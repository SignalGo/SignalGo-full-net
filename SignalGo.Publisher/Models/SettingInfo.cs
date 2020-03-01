using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.Publisher.Models
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
        public Guid ProjectKey { get; set; }
        public ObservableCollection<ProjectInfo> ProjectInfo { get; set; } = new ObservableCollection<ProjectInfo>();

        public static SettingInfo LoadSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PublisherData.db");
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

        public static void SaveSettingInfo()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PublisherData.db");
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(Current), Encoding.UTF8);
        }
    }
}
