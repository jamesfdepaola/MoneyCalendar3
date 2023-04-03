using System.Windows;
using MoneyCalendar.ViewModels;

namespace MoneyCalendar.Windows
{
    public partial class BillsWindow : Window
    {
        public BillsViewModel BillsViewModel { get; private set; }

        public BillsWindow(CalendarWindow owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = this.BillsViewModel = new BillsViewModel(owner.CalendarViewModel);
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            this.BillsViewModel.SaveBillsCommand.Execute();
        }
    }
}
