using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.Shared;
using SignalGo.Shared.Log;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SignalGo.ServiceManager.Core.Models
{
    public class ServerInfo : BaseViewModel
    {
        [JsonIgnore]
        public ServerProcessBaseInfo CurrentServerBase { get; set; }
        [JsonIgnore]
        public ServerDetailsInfo Details { get; set; }
        [JsonIgnore]
        public Action ProcessStarted { get; set; }

        private Guid _ServerKey;
        private int _StartDelay = 0;
        private string _Name;
        private string _AssemblyPath;
        private ServerInfoStatus _Status = ServerInfoStatus.Stopped;
        private bool _AutoStartEnabled = true;


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
        public bool AutoStartEnabled
        {
            get
            {
                return _AutoStartEnabled;
            }
            set
            {
                _AutoStartEnabled = value;
                OnPropertyChanged(nameof(AutoStartEnabled));
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

        /// <summary>
        /// stop the service and process
        /// </summary>
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
                        // remove from service usage monitoring dic
                        this.Details.ServiceMemoryUsage = "0";
                        Helpers.ServerDetailsManager.StopEngine(this);
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
        /// <summary>
        /// delay starting the service
        /// </summary>
        public int StartDelay
        {
            get { return _StartDelay; }
            set
            {
                _StartDelay = value;
                OnPropertyChanged(nameof(StartDelay));
            }
        }
        /// <summary>
        /// check service path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="serviceKey"></param>
        /// <returns></returns>
        public static ServerInfo CheckServerPath(string filePath, Guid serviceKey)
        {
            var find = SettingInfo.Current.ServerInfo.FirstOrDefault(x => x.ServerKey == serviceKey);
            try
            {
                if (find == null)
                    throw new Exception($"Service {serviceKey} not found!");
                else if (Path.GetDirectoryName(find.AssemblyPath) != Path.GetDirectoryName(filePath))
                    throw new Exception($"Access to the path denied!");
            }
            catch
            {

            }
            return find;
        }
        public void Start()
        {
            AsyncActions.RunOnUI(async () =>
            {
                // delay starting of service based on user config
                await System.Threading.Tasks.Task.Delay(StartDelay * 1000);
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
                        // doing health check
                        // enable service monitor
                        Helpers.ServerDetailsManager.AddServer(this);
                        Helpers.ServerDetailsManager.Enable(this);
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

        #region Ignore Some MvvmGo Properties From Saving in file
        [JsonIgnore]
        public override bool IsBusy { get => base.IsBusy; set => base.IsBusy = value; }
        [JsonIgnore]
        public override MvvmGo.Models.ValidationMessageInfo FirstMessage { get => base.FirstMessage; }
        [JsonIgnore]
        public override string BusyContent { get => base.BusyContent; set => base.BusyContent = value; }
        [JsonIgnore]
        public override Action<string> BusyContentChangedAction { get => base.BusyContentChangedAction; set => base.BusyContentChangedAction = value; }
        [JsonIgnore]
        public override Action<bool, string> IsBusyChangedAction { get => base.IsBusyChangedAction; set => base.IsBusyChangedAction = value; }
        [JsonIgnore]
        public override System.Collections.ObjectModel.ObservableCollection<MvvmGo.Models.ValidationMessageInfo> AllMessages { get => base.AllMessages; set => base.AllMessages = value; }
        [JsonIgnore]
        public override bool HasError { get => base.HasError; set => base.HasError = value; }
        [JsonIgnore]
        public override bool IsChangeBusyWhenCommandExecute { get => base.IsChangeBusyWhenCommandExecute; set => base.IsChangeBusyWhenCommandExecute = value; }
        [JsonIgnore]
        public override System.Collections.Concurrent.ConcurrentDictionary<string, MvvmGo.Models.ViewModelItemsInfo> MessagesByProperty { get => base.MessagesByProperty; set => base.MessagesByProperty = value; }
        [JsonIgnore]
        public override Action<string> PropertyChangedAction { get => base.PropertyChangedAction; set => base.PropertyChangedAction = value; }
        #endregion
    }
    public enum ServerInfoStatus
    {
        Started = 1,
        Stopped = 2,
        Updating = 3,
        Restarting = 4,
        Disabled = 5
    }
}
