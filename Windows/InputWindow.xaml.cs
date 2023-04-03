using System.Windows;

namespace MoneyCalendar.Windows
{
    public partial class InputWindow : Window
    {
        public InputWindow(string title, string message, string defaultinput = null)
        {
            InitializeComponent();

            this.Title = title;
            this.MessageTextBox.Text = message;
            this.InputTextBox.Text = defaultinput;
            this.InputTextBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            this.InputTextBox.Text = "";
        }
    }
}
