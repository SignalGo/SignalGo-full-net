using MvvmGo.ViewModels;
using Newtonsoft.Json;
using System;

namespace SignalGo.ServiceManager.Core.Models
{
    public class UserSetting : BaseViewModel
    {
        public UserSetting()
        {

        }

        private string _DotNetPath;
        private string _BackupPath;
        private string _LoggerPath;
        private string _ListeningPort;
        private string _ListeningAddress;
        private string _StartPriority;
        private string _ServiceUpdaterLogFilePath;

        public string ServiceUpdaterLogFilePath
        {
            get { return _ServiceUpdaterLogFilePath; }
            set
            {
                _ServiceUpdaterLogFilePath = value;
                OnPropertyChanged(nameof(ServiceUpdaterLogFilePath));
            }
        }
        public string LoggerPath
        {
            get { return _LoggerPath; }
            set
            {
                _LoggerPath = value;
                OnPropertyChanged(nameof(LoggerPath));
            }
        }
        public string ListeningAddress
        {
            get { return _ListeningAddress; }
            set
            {
                _ListeningAddress = value;
                OnPropertyChanged(nameof(ListeningAddress));
            }
        }
        public string StartPriority
        {
            get { return _StartPriority; }
            set
            {
                _StartPriority = value;
                OnPropertyChanged(nameof(StartPriority));
            }
        }
        public string ListeningPort
        {
            get { return _ListeningPort; }
            set
            {
                _ListeningPort = value;
                OnPropertyChanged(nameof(ListeningPort));
            }
        }

        public string BackupPath
        {
            get { return _BackupPath; }
            set
            {
                _BackupPath = value;
                OnPropertyChanged(nameof(BackupPath));
            }
        }
        
        public string DotNetPath
        {
            get { return _DotNetPath; }
            set
            {
                _DotNetPath = value;
                OnPropertyChanged(nameof(DotNetPath));
            }
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
}
