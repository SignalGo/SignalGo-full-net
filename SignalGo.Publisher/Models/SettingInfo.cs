using Newtonsoft.Json;
using SignalGo.Publisher.Extensions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.Publisher.Models
{
    /// <summary>
    /// Publisher Projects Setting 
    /// </summary>
    public class SettingInfo
    {
        const string PublisherDbName = "PublisherData.json";

        private static SettingInfo _Current = null;

        public static SettingInfo Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = LoadSettingInfo();
                    SaveSettingInfo();
                }
                return _Current;
            }
        }

        public ObservableCollection<ProjectInfo> ProjectInfo { get; set; } = new ObservableCollection<ProjectInfo>();

        public static SettingInfo LoadSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherDbName);
                if (!File.Exists(path) || File.ReadAllLinesAsync(path).Result.Length <= 0)
                {
                    File.Delete(path);
                    return new SettingInfo()
                    {
                        ProjectInfo = new ObservableCollection<ProjectInfo>(),
                    };
                }
                var result = SignalGo.Client.ClientSerializationHelper.DeserializeObject<SettingInfo>(File.ReadAllText(path, Encoding.UTF8));
                if (!result.HasValue())
                {
                    return new SettingInfo()
                    {
                        ProjectInfo = new ObservableCollection<ProjectInfo>(),
                    };
                }
                return result;
            }
            catch (Exception ex)
            {
                return new SettingInfo()
                {
                    ProjectInfo = new ObservableCollection<ProjectInfo>(),
                };
            }
        }
        public static void SaveSettingInfo()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublisherDbName);
            File.WriteAllText(path, JsonConvert.SerializeObject(Current, Formatting.Indented), Encoding.UTF8);
        }
    }
}
