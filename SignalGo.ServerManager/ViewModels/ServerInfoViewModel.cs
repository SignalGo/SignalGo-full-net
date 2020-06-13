using Microsoft.Win32;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.Core.Models;

namespace SignalGo.ServerManager.ViewModels
{
    public class ServerInfoViewModel : ServerInfoBaseViewModel
    {
        protected override void Delete()
        {
            MainWindowViewModel.MainFrame.GoBack();
            base.Delete();
        }
        protected override void BrowsePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = ServerInfo.AssemblyPath;
            if (fileDialog.ShowDialog().GetValueOrDefault())
                ServerInfo.AssemblyPath = fileDialog.FileName;
            SettingInfo.SaveSettingInfo();
        }

        //protected override void Copy(TextLogInfo textLogInfo)
        //{
        //    System.Windows.Forms.Clipboard.SetText(textLogInfo.Text);
        //}
    }
}
