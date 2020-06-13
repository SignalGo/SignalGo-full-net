using Microsoft.Win32;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.Core.Models;
using System.IO;
using System.Linq;
using System.Windows;

namespace SignalGo.ServerManager.ViewModels
{
    public class AddNewServerViewModel : AddNewServerBaseViewModel
    {
        protected override void BrowsePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = AssemblyPath;
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                AssemblyPath = fileDialog.FileName;
            }
        }

        protected override void Save()
        {
            if (string.IsNullOrEmpty(Name))
                MessageBox.Show("Plase set name of server");
            else if (!File.Exists(AssemblyPath))
                MessageBox.Show("Assembly file not exist on disk");
            else if (SettingInfo.Current.ServerInfo.Any(x => x.Name == Name))
                MessageBox.Show("Server name exist on list, please set a different name");
            else
            {
                SaveBase();
                Cancel();
            }
        }

        protected override void Cancel()
        {
            MainWindowViewModel.MainFrame.GoBack();
        }
    }
}
