using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;

namespace SignalGo.ServiceManager.Core.Models
{
    //public class ConsoleWriter : TextWriter
    //{
    //    public string ServerName { get; set; }
    //    public Action<string, string> TextAddedAction { get; set; }

    //    public ConsoleWriter()
    //    {
    //    }

    //    public override void Write(char value)
    //    {
    //        try
    //        {
    //            TextAddedAction?.Invoke(ServerName, value.ToString());
    //        }
    //        catch (Exception ex)
    //        {
    //            AutoLogger.Default.LogError(ex, "Write char");
    //        }
    //    }

    //    /// <summary>
    //    /// write action
    //    /// </summary>
    //    /// <param name="value"></param>
    //    public override void Write(string value)
    //    {
    //        try
    //        {
    //            TextAddedAction?.Invoke(ServerName, value);
    //        }
    //        catch (Exception ex)
    //        {
    //            AutoLogger.Default.LogError(ex, "Write string");
    //        }
    //    }

    //    public override Encoding Encoding
    //    {
    //        get { return Encoding.UTF8; }
    //    }
    //}

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
        //[JsonIgnore]
        //public ObservableCollection<TextLogInfo> Logs { get; set; } = new ObservableCollection<TextLogInfo>();

        [JsonIgnore]
        public ServerProcessBaseInfo CurrentServerBase { get; set; }

        [JsonIgnore]
        public Action ProcessStarted { get; set; }

        private Guid _ServerKey;
        private string _Name;
        private int _StartDelay = 0;
        private string _AssemblyPath;
        private ServerInfoStatus _Status = ServerInfoStatus.Stopped;


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
            get => _Name;
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
                return _Status;
            }
            set
            {
                _Status = value;
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
                        Console.WriteLine(ex);
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

        public static Action<Process> SendToMainHostForHidden { get; set; }
        public int StartDelay
        {
            get { return _StartDelay; }
            set
            {
                _StartDelay = value;
                OnPropertyChanged(nameof(StartDelay));
            }
        }

        public void Start()
        {
            // delay starting of service based on user config
            //Task.Delay(StartDelay * 1000);
            AsyncActions.RunOnUI(() =>
            {
                // if server status is Stopped
                if (Status == ServerInfoStatus.Stopped && Status != ServerInfoStatus.Disabled)
                {
                    try
                    {
                       
                        // set server status to Started
                        Status = ServerInfoStatus.Started;
                        CurrentServerBase = ServerProcessBaseInfo.Instance();
                        // start the server from the path
                        CurrentServerBase.Start("App_" + Name, AssemblyPath);
                        var process = CurrentServerBase.BaseProcess;
                        // Insert/Merge Servers Console Window to Server manager Windows Tab
                        SendToMainHostForHidden?.Invoke(process);
                        ProcessStarted?.Invoke();
                        Console.WriteLine($"Health Check, OK.");
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
