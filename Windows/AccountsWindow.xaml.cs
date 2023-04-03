using System.Windows;
using MoneyCalendar.ViewModels;

namespace MoneyCalendar.Windows
{
    public partial class AccountsWindow : Window
    {
        public AccountsViewModel AccountsViewModel { get; private set; }

        public AccountsWindow(CalendarWindow owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = this.AccountsViewModel = new AccountsViewModel(owner.CalendarViewModel);
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            this.AccountsViewModel.SaveAccountsCommand.Execute();
        }
    }
}
