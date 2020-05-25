using MvvmGo.ViewModels;

namespace SignalGo.ServiceManager.Models
{
    public class UserSetting : BaseViewModel
    {
        public UserSetting()
        {

        }

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

    }
}
