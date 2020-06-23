using SignalGo.ServiceManager.Core.BaseViewModels;
using System.Windows.Forms;

namespace SignalGo.ServerManager.WpfApp.ViewModels
{
    public class ServerManagerSettingsViewModel : ServerManagerSettingsBaseViewModel
    {
        protected override void BrowseBackupPath()
        {
            using FolderBrowserDialog BrowseBackupPathDialog = new FolderBrowserDialog();
            BrowseBackupPathDialog.SelectedPath = BrowseBackupPathDialog.SelectedPath;
            if (BrowseBackupPathDialog.ShowDialog() == DialogResult.OK)
                CurrentUserSettingInfo.UserSettings.BackupPath = BrowseBackupPathDialog.SelectedPath;
        }

        protected override void BrowseLoggerPath()
        {
            using FolderBrowserDialog BrowseLoggerPathDialog = new FolderBrowserDialog();
            BrowseLoggerPathDialog.SelectedPath = BrowseLoggerPathDialog.SelectedPath;
            if (BrowseLoggerPathDialog.ShowDialog() == DialogResult.OK)
                CurrentUserSettingInfo.UserSettings.LoggerPath = BrowseLoggerPathDialog.SelectedPath;
        }

        protected override void Back()
        {
            MainWindowViewModel.MainFrame.GoBack();
        }
    }
}
