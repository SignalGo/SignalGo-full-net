using MvvmGo.Commands;
using MvvmGo.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace SignalGo.ServerManager.ViewModels
{
    public class ServerManagerSettingsViewModel : BaseViewModel
    {

        public ServerManagerSettingsViewModel()
        {
            SaveCommand = new Command(Save);

        }

        public Command SaveCommand { get; set; }

        public void Save()
        {
            ConfigurationManager.AppSettings["BackupPath"] = BackupPath;
            ConfigurationManager.AppSettings["MsBuildPath"] = MsBuildPath;
            ConfigurationManager.AppSettings["ApplicationAutoLoggerFilePath"] = LoggerPath;
        }
        private string _MsBuildPath;

        public string MsBuildPath
        {
            get { return _MsBuildPath; }
            set
            {
                _MsBuildPath = value;
                OnPropertyChanged(nameof(MsBuildPath));
            }
        }
        private string _LoggerPath;

        public string LoggerPath
        {
            get { return _LoggerPath; }
            set
            {
                _LoggerPath = value;
                OnPropertyChanged(nameof(LoggerPath));
            }
        }

        private string _BackupPath;

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
