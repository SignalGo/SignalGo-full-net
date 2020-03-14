using Microsoft.Win32;
using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace SignalGo.ServerManager.ViewModels
{
    public class AddNewProjectViewModel : BaseViewModel
    {
        public AddNewProjectViewModel()
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
        Guid _ProjectKey = Guid.NewGuid();

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

        public Guid ProjectKey
        {
            get => _ProjectKey;
            set
            {
                _ProjectKey = value;
                OnPropertyChanged(nameof(ProjectKey));
            }
        }

        private void Cancel()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }

        private void BrowsePath()
        {
            //OpenFileDialog fileDialog = new OpenFileDialog();
            //fileDialog.FileName = AssemblyPath;
            //if (fileDialog.ShowDialog().GetValueOrDefault())
            //{
            //    AssemblyPath = fileDialog.FileName;
            //}

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = folderBrowserDialog.SelectedPath;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.AssemblyPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void Save()
        {

            if (string.IsNullOrEmpty(this.Name))
                System.Windows.MessageBox.Show("Plase set name of project");
            else if (!Directory.Exists(AssemblyPath))
                System.Windows.MessageBox.Show("files not exist on disk");
            else if (SettingInfo.Current.ProjectInfo.Any(x => x.Name == Name))
                System.Windows.MessageBox.Show("Project name exist on list, please set a different name");
            else
            {
                SettingInfo.Current.ProjectInfo.Add(new ProjectInfo()
                {
                    ProjectKey = this.ProjectKey,
                    AssemblyPath = AssemblyPath,
                    Name = Name,
                });
                SettingInfo.SaveSettingInfo();
                ProjectManagerWindowViewModel.MainFrame.GoBack();
            }
        }
    }
}
