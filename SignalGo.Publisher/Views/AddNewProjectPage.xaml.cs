using SignalGo.Publisher.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace SignalGo.Publisher.Views
{
    /// <summary>
    /// Interaction logic for AddNewProjectPage.xaml
    /// </summary>
    public partial class AddNewProjectPage : Page
    {
        List<string> Projects = new List<string>();
        public AddNewProjectPage()
        {
            InitializeComponent();
            AddProjectBase addProject = null;
            addProject = new AddProjectBase()
            {
                AddProjectCommand = new Helpers.RelayCommand(() =>
                {
                    AddProject(addProject.ProjectName);
                }, () =>
                {
                    return !string.IsNullOrEmpty(addProject.ProjectName) && (from x in Projects where x.ToLower() == addProject.ProjectName.ToLower() select x).FirstOrDefault() == null;
                })
            };
            foreach (var item in System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                var ext = System.IO.Path.GetExtension(item);
                if (ext == ".pdt")
                {
                    AddProject(System.IO.Path.GetFileNameWithoutExtension(item));
                }
            }
            //addProjectStack.DataContext = addProject;
        }

        public void AddProject(string ProjectName)
        {

            //mainTabControl.Items.Insert(mainTabControl.Items.Count - 1, new TabItem() { Header = ProjectName, Content = ctrl });
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }

    public class AddProjectBase
    {
        string _ProjectName = "";

        public string ProjectName
        {
            get
            {
                return _ProjectName;
            }
            set
            {
                _ProjectName = value;
            }
        }

        public RelayCommand AddProjectCommand { get; set; }
    }
}
