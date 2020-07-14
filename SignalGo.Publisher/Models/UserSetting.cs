using Newtonsoft.Json;
using System;
using System.Windows;

namespace SignalGo.Publisher.Models
{
    public class UserSetting : MvvmGo.ViewModels.BaseViewModel
    {
        public UserSetting()
        {

        }
        [Flags]
        public enum TestRunnersEnum
        {
            NetCoreSDK = 1,
            VsTestConsole = 2,
            UserDefined = 3
        }
        [Flags]
        public enum LoggingVerbosityEnum
        {
            Full = 1,
            Minimuum = 2,
            Quiet = 3
        }

        private bool _IsUiVirtualization;
        private string _MsbuildPath;
        private string _TestRunnerExecutableFile;
        private string _LoggerPath;
        private string _CommandRunnerLogsPath;
        private string _StartPriority;
        private string _ServiceUpdaterLogFilePath;

        private TestRunnersEnum _DefaultTestRunner = TestRunnersEnum.NetCoreSDK;
        private LoggingVerbosityEnum _LoggingVerbosity = LoggingVerbosityEnum.Minimuum;

        public TestRunnersEnum DefaultTestRunner
        {
            get { return _DefaultTestRunner; }
            set
            {
                _DefaultTestRunner = value;
                OnPropertyChanged(nameof(DefaultTestRunner));
            }
        }
        /// <summary>
        /// amount of output logs verbosity
        /// </summary>
        public LoggingVerbosityEnum LoggingVerbosity
        {
            get { return _LoggingVerbosity; }
            set
            {
                _LoggingVerbosity = value;
                OnPropertyChanged(nameof(LoggingVerbosity));
            }
        }

        //private bool _IsAccessControlUnlocked = false;
        //public bool IsAccessControlUnlocked
        //{
        //    get
        //    {
        //        return
        //          _IsAccessControlUnlocked;
        //    }
        //    set
        //    {
        //        _IsAccessControlUnlocked = value;
        //        OnPropertyChanged(nameof(IsAccessControlUnlocked));
        //    }
        //}

        public string TestRunnerExecutableFile
        {
            get { return _TestRunnerExecutableFile; }
            set
            {
                _TestRunnerExecutableFile = value;
                OnPropertyChanged(nameof(TestRunnerExecutableFile));
            }
        }
        public string ServiceUpdaterLogFilePath
        {
            get { return _ServiceUpdaterLogFilePath; }
            set
            {
                _ServiceUpdaterLogFilePath = value;
                OnPropertyChanged(nameof(ServiceUpdaterLogFilePath));
            }
        }
        public string CommandRunnerLogsPath
        {
            get { return _CommandRunnerLogsPath; }
            set
            {
                _CommandRunnerLogsPath = value;
                OnPropertyChanged(nameof(CommandRunnerLogsPath));
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
        public string MsbuildPath
        {
            get { return _MsbuildPath; }
            set
            {
                _MsbuildPath = value;
                OnPropertyChanged(nameof(MsbuildPath));
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

        private bool _IsDebug;
        private bool _IsRelease;
        private bool _IsRebuild;
        private bool _IsBuild;
        private bool _IsRestore;
        private int _MaxThreads;

        public int MaxThreads
        {
            get
            {
                return _MaxThreads;
            }
            set
            {
                if (value < Environment.ProcessorCount)
                {
                    MessageBox.Show("Value is smaller than you'r processor cores");
                    _MaxThreads = Environment.ProcessorCount;
                }
                else
                {
                    _MaxThreads = value;
                    OnPropertyChanged(nameof(MaxThreads));
                }

            }
        }
        public bool IsDebug
        {
            get
            {
                return _IsDebug;
            }
            set
            {
                _IsDebug = value;
                OnPropertyChanged(nameof(IsDebug));

            }
        }
        public bool IsRelease
        {
            get
            {
                return _IsRelease;
            }
            set
            {
                _IsRelease = value;
                OnPropertyChanged(nameof(IsRelease));

            }
        }
        public bool IsRebuild
        {
            get
            {
                return _IsRebuild;
            }
            set
            {
                _IsRebuild = value;
                OnPropertyChanged(nameof(IsRebuild));

            }
        }
        public bool IsRestore
        {
            get
            {
                return _IsRestore;
            }
            set
            {
                _IsRestore = value;
                OnPropertyChanged(nameof(IsRestore));

            }
        }

        /// <summary>
        /// setting used in ui list's and big item collection's. 
        /// true will optimize memory usage (but decrease performancce) and trigger GC every items change
        /// false will cache objects to memory and increase memory usage but better performance and ui reactions
        /// </summary>
        public bool IsUiVirtualization
        {
            get
            {
                return _IsUiVirtualization;
            }
            set
            {
                _IsUiVirtualization = value;
                OnPropertyChanged(nameof(IsUiVirtualization));

            }
        }
        public bool IsBuild
        {
            get
            {
                return _IsBuild;
            }
            set
            {
                _IsBuild = value;
                OnPropertyChanged(nameof(IsBuild));

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
