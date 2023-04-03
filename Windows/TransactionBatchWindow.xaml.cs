using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MoneyCalendar.ViewModels;

namespace MoneyCalendar.Windows
{
    public partial class TransactionBatchWindow : Window
    {
        public NewTransactionBatchViewModel BatchViewModel { get; private set; }

        public TransactionBatchWindow(CalendarWindow owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = this.BatchViewModel = new NewTransactionBatchViewModel(owner.CalendarViewModel);
            this.BatchViewModel.StartNewTransactionBatchCommand?.Execute();
        }

        private void AutoSelectAllTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
                textbox.SelectAll();
        }

        private void AutoCompleteTextoBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupSuggestion.IsOpen = false;
        }
    }
}