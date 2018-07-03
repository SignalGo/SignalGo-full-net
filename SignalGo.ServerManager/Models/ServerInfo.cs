using MvvmGo.ViewModels;
using SignalGo.ServerManager.Helpers;
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
            TextAddedAction?.Invoke(ServerName, value.ToString());
        }

        public override void Write(string value)
        {
            TextAddedAction?.Invoke(ServerName, value);
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
        public ObservableCollection<TextLogInfo> Logs { get; set; } = new ObservableCollection<TextLogInfo>();

        public ServerInfoBase CurrentServerBase { get; set; }
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
