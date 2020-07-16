using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Extensions;
using SignalGo.Publisher.Models;
using System.Linq;

namespace SignalGo.Publisher.ViewModels
{
    public class AddNewCategoryViewModel : BaseViewModel
    {
        private string _Name;
        private int _ParentID;

        public AddNewCategoryViewModel()
        {
            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);
        }

        public Command SaveCommand { get; set; }
        public Command CancelCommand { get; set; }

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public int ParentID
        {
            get => _ParentID;
            set
            {
                _ParentID = value;
                OnPropertyChanged(nameof(ParentID));
            }
        }


        private void Save()
        {
            if (Name.HasValue())
            {
                //SettingInfo.Current.CategoryInfos.Add(new CategoryInfo()
                //{
                //    ID = (SettingInfo.Current.CategoryInfos.Last().ID) + 1,
                //    ParentID = ParentID,
                //    Name = this.Name,
                //});
                SettingInfo.SaveSettingInfo();
                ProjectManagerWindowViewModel.MainFrame.GoBack();
            }
            else
            {
                System.Windows.MessageBox.Show("Please set the name of category!");
                return;
            }

        }

        private void Cancel()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();
        }
    }
}
