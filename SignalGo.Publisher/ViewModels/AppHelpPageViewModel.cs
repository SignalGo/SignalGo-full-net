using MvvmGo.Commands;
using MvvmGo.ViewModels;

namespace SignalGo.Publisher.ViewModels
{
    public class AppHelpPageViewModel : BaseViewModel
    {

        public AppHelpPageViewModel()
        {
            BackCommand = new Command(Back);
        }
        public Command BackCommand { get; set; }
        public void Back()
        {
            ProjectManagerWindowViewModel.MainFrame.GoBack();
            //ProjectManagerWindow.This.mainframe.Navigate(new AppHelpPage());

        }
    }
}
