using System;
using System.Windows;

namespace SignalGo.Publisher.Views.Extra
{
    /// <summary>
    /// Interaction logic for InputDialogWindow.xaml
    /// </summary>
    public partial class InputDialogWindow : Window
    {
        public InputDialogWindow(string question, string title = "", string importantText = "", string hintText = "", string defaultAnswer = "")
        {
            InitializeComponent();
            Title = title;
            lblQuestion.Text = question;
            lblImportantText.Text = importantText;
            txtAnswer.Password = defaultAnswer;
            txtHintText.Text = hintText;
        }
        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();
        }
        public string Answer
        {
            get { return txtAnswer.Password; }
        }
    }
}
