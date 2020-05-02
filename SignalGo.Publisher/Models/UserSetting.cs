using MvvmGo.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Documents;

namespace SignalGo.Publisher.Models
{
    public class UserSetting : BaseViewModel
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

        private string _MsbuildPath;
        private string _TestRunnerExecutableFile;
        private string _LoggerPath;
        private string _CommandRunnerLogsPath;
        private string _StartPriority;

        private string _ServiceUpdaterLogFilePath;
        private TestRunnersEnum _DefaultTestRunner = TestRunnersEnum.NetCoreSDK;

        public TestRunnersEnum DefaultTestRunner
        {
            get { return _DefaultTestRunner; }
            set
            {
                _DefaultTestRunner = value;
                OnPropertyChanged(nameof(DefaultTestRunner));
            }
        }
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
                    //MessageBox.Show("Value is smaller than you'r processor cores");
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

        //public enum CompileOptionsEnum
        //{
        //    Restore = 1,
        //    Clean = 2,
        //    Build = 3,
        //    Rebuild = 4,
        //    Debug = 5,
        //    Release = 6,
        //}
    }
}
