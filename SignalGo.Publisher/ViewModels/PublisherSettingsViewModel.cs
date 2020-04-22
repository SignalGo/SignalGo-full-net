using System;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using System.Diagnostics;
using SignalGo.Publisher.Models;
using System.IO;
using System.Windows.Forms;

namespace SignalGo.Publisher.ViewModels
{
    public class PublisherSettingsViewModel : BaseViewModel
    {
        public PublisherSettingsViewModel()
        {
            SaveCommand = new Command(Save);
            BackCommand = new Command(Back);
            BrowseMsbuildPathCommand = new Command(BrowseMsbuildPath);
            BrowseLoggerPathCommand = new Command(BrowseLoggerPath);
        }

        public Command BrowseMsbuildPathCommand { get; set; }
        public Command BrowseLoggerPathCommand { get; set; }
        public Command RestoreDefaults { get; set; }
        public Command SaveCommand { get; set; }
        public Command BackCommand { get; set; }

        private void BrowseMsbuildPath()
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.FileName = UserSettingInfo.Current.UserSettings.MsbuildPath;
            if (fileDialog.ShowDialog().GetValueOrDefault())
                UserSettingInfo.Current.UserSettings.MsbuildPath = fileDialog.FileName;
        }

        private void BrowseLoggerPath()
        {
            using FolderBrowserDialog BrowseLoggerPathDialog = new FolderBrowserDialog();
            BrowseLoggerPathDialog.SelectedPath = BrowseLoggerPathDialog.SelectedPath;
            if (BrowseLoggerPathDialog.ShowDialog() == DialogResult.OK)
                CurrentUserSettingInfo.UserSettings.LoggerPath = BrowseLoggerPathDialog.SelectedPath;
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

        public void Back()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }
        public void Save()
        {
            try
            {
                UserSettingInfo.SaveUserSettingInfo();
                ProjectManagerWindowViewModel.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
