using Microsoft.Win32;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServerManager.Models;
using System;
using System.IO;
using System.Linq;
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
        Guid _ServerKey;

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
        public Guid ServerKey
        {
            get
            {
                if (_ServerKey != Guid.Empty)
                {
                    return _ServerKey;
                }
                else
                {
                    _ServerKey = Guid.NewGuid();
                    return _ServerKey;
                }
            }
            set
            {
                _ServerKey = value;
                OnPropertyChanged(nameof(ServerKey));
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
            else if (SettingInfo.Current.ServerInfo.Any(x => x.Name == Name))
                MessageBox.Show("Server name exist on list, please set a different name");
            else
            {
                SettingInfo.Current.ServerInfo.Add(new ServerInfo()
                {
                    AssemblyPath = AssemblyPath,
                    Name = Name,
                    ServerKey = ServerKey
                });
                SettingInfo.SaveSettingInfo();
                MainWindowViewModel.MainFrame.GoBack();
            }
        }
    }
}
