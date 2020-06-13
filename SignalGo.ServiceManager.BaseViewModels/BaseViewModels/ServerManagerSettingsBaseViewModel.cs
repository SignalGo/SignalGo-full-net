using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServiceManager.Core.Models;
using System;
using System.Diagnostics;

namespace SignalGo.ServiceManager.Core.BaseViewModels
{
    public class ServerManagerSettingsBaseViewModel : BaseViewModel
    {
        public Command BrowseBackupPathCommand { get; set; }
        public Command BrowseLoggerPathCommand { get; set; }
        public Command RestoreDefaults { get; set; }
        public Command SaveCommand { get; set; }
        public Command BackCommand { get; set; }
        public ServerManagerSettingsBaseViewModel()
        {
            SaveCommand = new Command(Save);
            BackCommand = new Command(Back);
            BrowseBackupPathCommand = new Command(BrowseBackupPath);
            BrowseLoggerPathCommand = new Command(BrowseLoggerPath);
        }


        protected virtual void BrowseBackupPath()
        {
        }

        protected virtual void BrowseLoggerPath()
        {
        }

        public UserSettingInfo CurrentUserSettingInfo
        {
            get
            {
                return UserSettingInfo.Current;
            }
        }

        UserSetting _UserSetting;
        public UserSetting UserSetting
        {
            get
            {
                return _UserSetting;
            }

            set
            {
                _UserSetting = value;
                OnPropertyChanged(nameof(UserSetting));
            }
        }

        protected virtual void Back()
        {

        }
        public void Save()
        {
            try
            {
                UserSettingInfo.SaveUserSettingInfo();
                Back();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }
        }
    }
}
