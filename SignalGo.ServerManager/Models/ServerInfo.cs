using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.ServerManager.Views;
using SignalGo.Shared.Log;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

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
        private string _Text;
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

        private string _Name;
        private string _AssemblyPath;
        private ServerInfoStatus _status = ServerInfoStatus.Stopped;
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

        public void Stop()
        {
            if (Status == ServerInfoStatus.Started)
            {
                while (true)
                {
                    try
                    {
                        CurrentServerBase.Dispose();
                        CurrentServerBase = null;
                        Status = ServerInfoStatus.Stopped;
                        break;
                    }
                    catch (Exception ex)
                    {
                        AutoLogger.Default.LogError(ex, "Stop Server");
                    }
                    finally
                    {
                        GC.Collect();
                        GC.WaitForFullGCComplete();
                        GC.Collect();
                    }
                }
            }
        }

        public void Start()
        {
            MainWindow.This.Dispatcher.Invoke(() =>
            {
                if (Status == ServerInfoStatus.Stopped)
                {
                    try
                    {
                        Status = ServerInfoStatus.Started;
                        CurrentServerBase = new ServerProcessInfoBase();
                        CurrentServerBase.Start("App_" + Name, AssemblyPath);
                        ServerInfoPage.SendToMainHostForHidden(CurrentServerBase.BaseProcess,null);
                        ProcessStarted?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        AutoLogger.Default.LogError(ex, "StartServer");
                        if (CurrentServerBase != null)
                        {
                            CurrentServerBase.Dispose();
                            CurrentServerBase = null;
                        }
                        Status = ServerInfoStatus.Stopped;
                    }
                    SettingInfo.SaveSettingInfo();
                }
            });
        }
    }
}
