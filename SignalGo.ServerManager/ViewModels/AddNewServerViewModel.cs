using Microsoft.Win32;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServerManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignalGo.ServerManager.ViewModels
{
    public class AddNewServerViewModel : BaseViewModel
    {
        public AddNewServerViewModel()
        {
            CancelCommand = new Command(Cancel);
            SaveCommand = new Command(Save);
            BrowsePathCommand = new Command(BrowsePath);
        }

        public Command CancelCommand { get; set; }
        public Command SaveCommand { get; set; }
        public Command BrowsePathCommand { get; set; }

        string _Name;
        string _AssemblyPath;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string AssemblyPath
        {
            get
            {
                return _AssemblyPath;
            }
            set
            {
                _AssemblyPath = value;
                OnPropertyChanged(nameof(AssemblyPath));
            }
        }

        private void Cancel()
        {
            MainWindowViewModel.MainFrame.GoBack();
        }

        private void BrowsePath()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = AssemblyPath;
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                AssemblyPath = fileDialog.FileName;
            }
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(Name))
                MessageBox.Show("Plase set name of server");
            else if (!File.Exists(AssemblyPath))
                MessageBox.Show("Assembly file not exist on disk");
            else if (SettingInfo.Current.ServerInfoes.Any(x => x.Name == Name))
                MessageBox.Show("Server name exist on list, please set a different name");
            else
            {
                SettingInfo.Current.ServerInfoes.Add(new Models.ServerInfo() { AssemblyPath = AssemblyPath, Name = Name });
                SettingInfo.SaveSettingInfo();
                MainWindowViewModel.MainFrame.GoBack();
            }
        }
    }
}
