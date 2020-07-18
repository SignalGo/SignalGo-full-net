using System;
using System.Windows;
using MvvmGo.ViewModels;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.ViewModels;
using SignalGo.Publisher.Views;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using SignalGo.Shared;
using SignalGo.Publisher.Shared.Helpers;
using SignalGo.Shared.Log;

namespace SignalGo.Publisher
{
    /// <summary>
    /// Interaction logic for ProjectManagerWindow.xaml
    /// </summary>
    public partial class ProjectManagerWindow : Window
    {
        public static ProjectManagerWindow This;

        public ProjectManagerWindow()
        {
            BaseViewModel.Initialize();
            BaseViewModel.RunOnUIAction = (x) =>
            {
                Dispatcher.BeginInvoke(x);
            };
            AsyncActions.InitializeUIThread();
            This = this;
            InitializeComponent();
            Closing += (s, e) =>
            {
                AutoLogger.Default.LogText("Try to close happens.");
                if (MessageBox.Show("Are you sure to close publisher? this will disconnect from all remote server's.", "Close application", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    AutoLogger.Default.LogText("Manualy user cancel closing.");
                    e.Cancel = true;
                    return;
                }
            };
            // this event execute code's after app loaded
            Loaded += (s, e) =>
            {
                DailyBackup.GetBackupFromAppLog(UserSettingInfo.Current.UserSettings.LoggerPath);
            };
            mainframe.Navigate(new FirstPage());
        }

        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var ta = new ThicknessAnimation();
            ta.Duration = TimeSpan.FromSeconds(0.3);
            ta.DecelerationRatio = 0.7;
            ta.To = new Thickness(0, 0, 0, 0);
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Forward)
            {
                ta.From = new Thickness(500, 0, -500, 0);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                ta.From = new Thickness(-500, 0, 500, 0);
            }
            else if (e.NavigationMode == NavigationMode.Refresh)
            {
                ta.From = new Thickness(0, 0, 0, 0);
            }

            (e.Content as Page).BeginAnimation(MarginProperty, ta);

        }
        private SolidColorBrush _DefaultColor = Brushes.DarkGray;

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectManagerWindowViewModel.MainFrame = (Frame)sender;
        }
        private void projectItem_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    var textSender = sender as TextBox;
                    var eventSource = e.OriginalSource as TextBox;
                    (eventSource.DataContext
                     as ProjectInfo).Name = (e.OriginalSource as TextBox).Text;
                    SettingInfo.SaveSettingInfo();
                    //new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    (textSender.Parent
                     as InlineUIContainer).Parent.SetValue(TextBlock.BackgroundProperty, Brushes.Green);
                    Task.Delay(3000).GetAwaiter().OnCompleted(() =>
                    {
                        (textSender.Parent
                        as InlineUIContainer).Parent.SetValue(TextBlock.BackgroundProperty, _DefaultColor);
                    });
                    break;
                case Key.Escape:
                    textSender = sender as TextBox;
                    eventSource = e.OriginalSource as TextBox;
                    eventSource.Undo();
                    (textSender.Parent
                     as InlineUIContainer).Parent.SetValue(TextBlock.BackgroundProperty, Brushes.IndianRed);
                    Task.Delay(3000).GetAwaiter().OnCompleted(() =>
                    {
                        (textSender.Parent
                         as InlineUIContainer).Parent.SetValue(TextBlock.BackgroundProperty, _DefaultColor);
                    });
                    break;
                default:
                    textSender = sender as TextBox;
                    (textSender.Parent
                     as InlineUIContainer).Parent.SetValue(TextBlock.BackgroundProperty, Brushes.Yellow);
                    break;
            }
        }

    }
}
