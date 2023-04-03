using System.Windows;
using System.Windows.Controls;
using MoneyCalendar.ViewModels;

namespace MoneyCalendar.Windows
{
    public partial class LoadDueDatesWindow : Window
    {
        public LoadDueDatesViewModel LoadDueDatesViewModel { get; private set; }

        public LoadDueDatesWindow(CalendarWindow owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.DataContext = this.LoadDueDatesViewModel = new LoadDueDatesViewModel(this, owner.CalendarViewModel);
        }
    }
}
