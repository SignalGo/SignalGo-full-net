using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.Views;
using SignalGo.Shared.Log;
using System;
using System.Windows.Controls;

namespace SignalGo.Publisher.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectManagerWindowViewModel : BaseViewModel
    {

        public static ProjectManagerWindowViewModel This { get; set; }

        public ProjectManagerWindowViewModel()
        {
            This = this;
            AddNewProjectCommand = new Command(AddNewProject);
            Load();
        }


        public Command AddNewProjectCommand { get; set; }

        public static Frame MainFrame { get; set; }

        private ProjectInfo _SelectedProjectInfo;

        public ProjectInfo SelectedProjectInfo
        {
            get
            {
                return _SelectedProjectInfo;
            }
            set
            {
                _SelectedProjectInfo = value;
                OnPropertyChanged(nameof(SelectedProjectInfo));
                ProjectInfoPage page = new ProjectInfoPage();
                ProjectInfoViewModel vm = page.DataContext as ProjectInfoViewModel;
                vm.ProjectInfo = value;
                MainFrame.Navigate(page);
            }
        }

        public SettingInfo CurrentSettingInfo
        {
            get
            {
                return SettingInfo.Current;
            }
        }

        private void AddNewProject()
        {
            MainFrame.Navigate(new AddNewProjectPage());

        }

        public void Load()
        {

            try
            {
                foreach (ProjectInfo project in SettingInfo.Current.ProjectInfo)
                {
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Load");
            }
        }



    }
}
