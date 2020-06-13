using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.ServiceManager.Core.Models;
using System;

namespace SignalGo.ServiceManager.Core.BaseViewModels
{
    public class AddNewServerBaseViewModel : BaseViewModel
    {
        public AddNewServerBaseViewModel()
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

        protected virtual void Cancel()
        {
            
        }

        protected virtual void BrowsePath()
        {

        }

        protected virtual void Save()
        {
            SaveBase();
        }

        protected void SaveBase()
        {
            SettingInfo.Current.ServerInfo.Add(new ServerInfo()
            {
                AssemblyPath = AssemblyPath,
                Name = Name,
                ServerKey = ServerKey
            });
            SettingInfo.SaveSettingInfo();
        }
    }
}
