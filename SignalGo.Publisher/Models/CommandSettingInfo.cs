using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SignalGo.Publisher.Models
{
    public class CommandSettingInfo
    {
        const string CommandsDbName = "CommandsData.json";

        private static CommandSettingInfo _Current = null;

        public static CommandSettingInfo Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = LoadCommandSettingInfo();
                    SaveCommandSettingInfo();
                }
                return _Current;
            }
        }

        //[JsonIgnore]
        //public Guid CommandKey { get; set; }

        public ObservableCollection<CommandSetting> CommandSettings { get; set; } = new ObservableCollection<CommandSetting>();

        public static CommandSettingInfo LoadCommandSettingInfo()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CommandsDbName);
                if (!File.Exists(path) || File.ReadAllLinesAsync(path).Result.Length <= 0)
                {
                    File.Delete(path);
                    return new CommandSettingInfo()
                    {
                        CommandSettings = new ObservableCollection<CommandSetting>()
                    };
                }
                return JsonConvert.DeserializeObject<CommandSettingInfo>(File.ReadAllText(path, Encoding.UTF8));
            }
            catch
            {
                return new CommandSettingInfo()
                {
                    CommandSettings = new ObservableCollection<CommandSetting>()
                };
            }
        }

        public static void SaveCommandSettingInfo()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CommandsDbName);
            File.WriteAllText(path, JsonConvert.SerializeObject(Current, Formatting.Indented), Encoding.UTF8);
        }
    }
}
