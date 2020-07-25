using System;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using System.Diagnostics;
using SignalGo.Publisher.Models;
using System.Windows.Forms;
using SignalGo.Publisher.Views.Extra;
using SignalGo.Publisher.Extensions;
using SignalGo.Publisher.Engines.Models;

namespace SignalGo.Publisher.ViewModels
{
    public class PublisherSettingsViewModel : BaseViewModel
    {
        public PublisherSettingsViewModel()
        {
            SaveCommand = new Command(Save);
            SetMasterPasswordCommand = new Command(SetMasterPassword);
            BackCommand = new Command(Back);
            BrowseMsbuildPathCommand = new Command(BrowseMsbuildPath);
            BrowseLoggerPathCommand = new Command(BrowseLoggerPath);
            BrowseTestRunnerCommand = new Command(BrowseTestRunner);
            BrowseCommandRunnerLogPathCommand = new Command(BrowseCommandRunnerLogPath);
        }

        public Command BrowseMsbuildPathCommand { get; set; }
        public Command BrowseLoggerPathCommand { get; set; }
        public Command BrowseCommandRunnerLogPathCommand { get; set; }
        public Command BrowseTestRunnerCommand { get; set; }
        public Command RestoreDefaults { get; set; }
        public Command SetMasterPasswordCommand { get; set; }
        public Command SaveCommand { get; set; }
        public Command BackCommand { get; set; }

        private void BrowseMsbuildPath()
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.FileName = CurrentUserSettingInfo.UserSettings.MsbuildPath;
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

        public void SetMasterPassword()
        {

            InputDialogWindow inputDialog = new InputDialogWindow($"Please enter your ", "Access Control", "MasterPassword");
            if (inputDialog.ShowDialog() == true && inputDialog.Answer.HasValue())
            {
                CurrentUserSettingInfo.UserSettings.ApplicationMasterPassword = PasswordEncoder.ComputeHash(inputDialog.Answer);
                SaveCommand.Execute();
            }
            else
            {
                MessageBox.Show("Couldn't Set Master Password.");
            }
        }
        public void BrowseCommandRunnerLogPath()
        {
            using FolderBrowserDialog BrowseLoggerPathDialog = new FolderBrowserDialog();
            BrowseLoggerPathDialog.SelectedPath = CurrentUserSettingInfo.UserSettings.CommandRunnerLogsPath;
            if (BrowseLoggerPathDialog.ShowDialog() == DialogResult.OK)
                CurrentUserSettingInfo.UserSettings.CommandRunnerLogsPath = BrowseLoggerPathDialog.SelectedPath;

        }
        public void BrowseTestRunner()
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.FileName = CurrentUserSettingInfo.UserSettings.TestRunnerExecutableFile;
            if (fileDialog.ShowDialog().GetValueOrDefault())
                UserSettingInfo.Current.UserSettings.TestRunnerExecutableFile = fileDialog.FileName;
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
