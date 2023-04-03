using System.Windows;
using System.Windows.Controls;
using MoneyCalendar.ViewModels;

namespace MoneyCalendar.Windows
{
    public partial class MonthlyExpensesWindow : Window
    {
        public MonthlyExpensesViewModel MonthlyExpensesViewModel { get; private set; }

        public MonthlyExpensesWindow(CalendarWindow owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = this.MonthlyExpensesViewModel = new MonthlyExpensesViewModel(owner.CalendarViewModel);
            this.MonthlyExpensesViewModel.CloseAction = this.Close;
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            
        }

        private void Include_Checked(object sender, RoutedEventArgs e)
        {            
            this.MonthlyExpensesViewModel.RecalculateCommand.Execute();


            //EventSetter suck = new EventSetter(TextBox.
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //System.Diagnostics.Debugger.Break();
        }
    }
}
