using SignalGo.Publisher.Models;
using System;
using System.Linq;
using System.Windows.Controls;

namespace SignalGo.Publisher.Views
{
    /// <summary>
    /// Interaction logic for PublisherSettingManagerPage.xaml
    /// </summary>
    public partial class PublisherSettingManagerPage : Page
    {
        public PublisherSettingManagerPage()
        {
            InitializeComponent();
            testRunnersCombo.ItemsSource = Enum.GetValues(typeof(UserSetting.TestRunnersEnum))
                .Cast<UserSetting.TestRunnersEnum>();

            logVerbosityCombo.ItemsSource = Enum.GetValues(typeof(UserSetting.LoggingVerbosityEnum))
                .Cast<UserSetting.LoggingVerbosityEnum>();
        }
    }
}
