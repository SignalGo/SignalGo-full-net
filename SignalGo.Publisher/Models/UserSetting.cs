using MvvmGo.ViewModels;
using System;

namespace SignalGo.Publisher.Models
{
    public class UserSetting : BaseViewModel
    {
        public UserSetting()
        {

        }

        private string _MsbuildPath;
        private string _LoggerPath;
        private string _CommandRunnerLogsPath;
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

    }
}
