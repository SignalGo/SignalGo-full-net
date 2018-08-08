using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.ServerManager.Models
{
    public class ConsoleWriter : TextWriter
    {
        public string ServerName { get; set; }
        public Action<string, string> TextAddedAction { get; set; }

        public ConsoleWriter()
        {
        }

        public override void Write(char value)
        {
            try
            {
                TextAddedAction?.Invoke(ServerName, value.ToString());
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Write char");
            }
        }

        public override void Write(string value)
        {
            try
            {
                TextAddedAction?.Invoke(ServerName, value);
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Write string");
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    public enum ServerInfoStatus : byte
    {
        Started = 1,
        Stopped = 2,
        Updating = 3
    }

    public class TextLogInfo : BaseViewModel
    {
        string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        public bool IsDone { get; set; }
    }

    public class ServerInfo : BaseViewModel
    {
        [JsonIgnore]
        public ObservableCollection<TextLogInfo> Logs { get; set; } = new ObservableCollection<TextLogInfo>();

        [JsonIgnore]
        public ServerProcessInfoBase CurrentServerBase { get; set; }
        [JsonIgnore]
        public Action ProcessStarted { get; set; }

        string _Name;
        string _AssemblyPath;
        ServerInfoStatus _status = ServerInfoStatus.Stopped;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string AssemblyPath
        {
            get
            {
                return _AssemblyPath;
            }
            set
            {
                _AssemblyPath = value;
                OnPropertyChanged(nameof(AssemblyPath));
            }
        }

        [JsonIgnore]
        public ServerInfoStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }
}
