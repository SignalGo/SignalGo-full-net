using Microsoft.Win32;
using SignalGo.ServiceManager.BaseViewModels;
using SignalGo.ServiceManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

        protected override void Copy(TextLogInfo textLogInfo)
        {
            System.Windows.Forms.Clipboard.SetText(textLogInfo.Text);
        }
    }
}
