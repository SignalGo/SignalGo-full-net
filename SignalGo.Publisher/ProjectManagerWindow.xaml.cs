using System;
using System.IO;
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
            mainframe.Navigate(new FirstPage());
            Closing += (s, e) =>
            {
                try
                {
                    //File.Delete(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath);
                }
                catch { }
            };
            // this event execute code's after app loaded
            Loaded += async (s, e) =>
            {
                    try
                    {
                        // check if application log files is for past, and then backup
                        string backupPath = Path.Combine(Environment.CurrentDirectory, "AppBackups", "Logs");
                        var fileDate = File.GetCreationTime(UserSettingInfo.Current.UserSettings.LoggerPath);

                        if (fileDate.Date < DateTime.Now.Date)
                        {
                            // backup logs to a new file with it's date
                            string outFileName = $"{Path.GetFileNameWithoutExtension(UserSettingInfo.Current.UserSettings.LoggerPath)}{fileDate: _MMddyyyy}.log";
                            File.Move(UserSettingInfo.Current.UserSettings.LoggerPath,
                                Path.Combine(backupPath, outFileName), false);
                            File.Create(UserSettingInfo.Current.UserSettings.LoggerPath).Dispose();
                        }
                        #region to multiple backup
                        //Dictionary<string, DateTime> logsInfo = new Dictionary<string, DateTime>();

                        //logsInfo.Add(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath, File.GetCreationTime(UserSettingInfo.Current.UserSettings.CommandRunnerLogsPath));
                        //logsInfo.Add(UserSettingInfo.Current.UserSettings.LoggerPath, File.GetCreationTime(UserSettingInfo.Current.UserSettings.LoggerPath));

                        //if (!Directory.Exists(backupPath))
                        //    Directory.CreateDirectory(backupPath);

                        //foreach (var item in logsInfo)
                        //{
                        //    //var day = item.Value.Date.Day;
                        //    if (item.Value.Date != DateTime.Now.Date)
                        //    {
                        //        // backup logs to a new file with it's date
                        //        string outFileName = $"{Path.GetFileNameWithoutExtension(item.Key)}{item.Value: _MMddyyyy}.log";
                        //        try
                        //        {
                        //            File.Copy(item.Key,
                        //                Path.Combine(backupPath, outFileName), true);
                        //        }
                        //        catch { }
                        //    }

                        //}
                        #endregion
                    }
                    catch (Exception ex)
                    {

                    }
            };
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
