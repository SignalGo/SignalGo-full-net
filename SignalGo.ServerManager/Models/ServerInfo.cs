using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using MvvmGo.ViewModels;
using SignalGo.Shared.Log;
using SignalGo.ServerManager.Views;
using System.Collections.ObjectModel;
using System.Windows;

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

        /// <summary>
        /// write action
        /// </summary>
        /// <param name="value"></param>
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
        Updating = 3,
        Restarting = 4,
        Disabled = 5
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
        private Guid _ServerKey;
       
        private string _Name;
        private string _AssemblyPath;
        private ServerInfoStatus _status = ServerInfoStatus.Stopped;
        public Guid ServerKey
        {
            get
            {
                if (_ServerKey != Guid.Empty)
                {
                    return _ServerKey;
                }
                else
                {
                    _ServerKey = Guid.NewGuid();
                    return _ServerKey;
                }
            }
            set
            {
                _ServerKey = value;
                OnPropertyChanged(nameof(ServerKey));
            }
        }
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
                        // release resources
                        CurrentServerBase.Dispose();
                        // null current server base process info
                        CurrentServerBase = null;
                        // ser server status to stopped
                        Status = ServerInfoStatus.Stopped;
                        // get out
                        break;
                    }
                    catch (Exception ex)
                    {
                        // if any exception occured
                        AutoLogger.Default.LogError(ex, "Stop Server");
                    }
                    // at last, call Grabage Collector to free memory
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
                // if server status is Stopped
                if (Status == ServerInfoStatus.Stopped)
                {
                    try
                    {
                        // set server status to Started
                        Status = ServerInfoStatus.Started;
                        CurrentServerBase = new ServerProcessInfoBase();
                        // start the server from the path
                        CurrentServerBase.Start("App_" + Name, AssemblyPath);
                        // Insert/Merge Servers Console Window to Server manager Windows Tab
                        ServerInfoPage.SendToMainHostForHidden(CurrentServerBase.BaseProcess, null);
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
                else
                    MessageBox.Show("Service aleready started!");
            });
        }
    }
}
