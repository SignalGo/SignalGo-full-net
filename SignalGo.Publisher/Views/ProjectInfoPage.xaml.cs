using System.Windows.Controls;

namespace SignalGo.Publisher.Views
{
    /// <summary>
    /// Interaction logic for ProjectInfoPage.xaml
    /// </summary>
    public partial class ProjectInfoPage : Page
    {
        /// <summary>
        /// Server Manager Servers Information Page
        /// </summary>
        public ProjectInfoPage()
        {
            InitializeComponent();
        }
        private void txtCmdLogs_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtCmdLogs.ScrollToEnd();
        }

        private void txtIgnoreServerFileName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }
}
