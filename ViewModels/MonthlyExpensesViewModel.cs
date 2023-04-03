using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MoneyCalendar.Data;
using MoneyCalendar.DataModels;
using Prism.Commands;

namespace MoneyCalendar.ViewModels
{
    public class MonthlyExpensesViewModel : ViewModelBase
    {
        #region Members
        private ObservableCollection<MonthlyExpense> _monthlyExpenses;
        private ObservableCollection<MonthlyExpenseSet> _monthlyExpenseSets;

        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _totalAmount;
        private decimal _totalSplitAmount;
        private decimal _totalOwedAmount;        
        #endregion

        #region Properties
        public CalendarViewModel CalendarViewModel { get; private set; }

        public ObservableCollection<MonthlyExpense> MonthlyExpenses { get => this._monthlyExpenses; private set => this.SetProperty(ref this._monthlyExpenses, value); }
        public ObservableCollection<MonthlyExpenseSet> MonthlyExpenseSets { get => this._monthlyExpenseSets; private set => this.SetProperty(ref this._monthlyExpenseSets, value); }

        public DateTime StartDate
        {
            get => this._startDate;
            set
            {
                this.SetProperty(ref this._startDate, value);
                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadMonthlyExpenses());
            }
        }
        public DateTime EndDate
        {
            get => this._endDate; set
            {
                this.SetProperty(ref this._endDate, value);
                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadMonthlyExpenses());
            }
        }

        public decimal TotalAmount { get => this._totalAmount; set => this.SetProperty(ref this._totalAmount, value); }
        public decimal TotalSplitAmount { get => this._totalSplitAmount; set => this.SetProperty(ref this._totalSplitAmount, value); }
        public decimal TotalOwedAmount { get => this._totalOwedAmount; set => this.SetProperty(ref this._totalOwedAmount, value); }

        public Action CloseAction { get; set; }
        #endregion

        #region Commands    
        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand RecalculateCommand { get; set; } 
        public DelegateCommand SaveMonthlyExpensesCommand { get; set; }
        public DelegateCommand CloseWithoutSaveCommand { get; private set; }
        #endregion

        #region Class Methods
        public MonthlyExpensesViewModel(CalendarViewModel calendarviewmodel) : base()
        {
            try
            {
                this.CalendarViewModel = calendarviewmodel;

                this._startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                this._endDate = this.StartDate.AddMonths(1).AddDays(-1);

                this.SetupCommands();

                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadMonthlyExpenseSets());
                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadMonthlyExpenses());
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void SetupCommands()
        {
            this.RefreshCommand = new DelegateCommand(async () => await this.LoadMonthlyExpenses());
            this.RecalculateCommand = new DelegateCommand(this.Recalculate);
            this.SaveMonthlyExpensesCommand = new DelegateCommand(this.SaveMonthlyExpenses);
            this.CloseWithoutSaveCommand = new DelegateCommand(() => this.CloseAction?.Invoke());
        }
        #endregion

        #region Data
        public void LoadMonthlyExpenseSets()
        {
            try
            {
                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    this.MonthlyExpenseSets = new ObservableCollection<MonthlyExpenseSet>(context.MonthlyExpenseSets.OrderByDescending(set => set.EndDate));
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
        
        public async Task LoadMonthlyExpenses()
        {
            try
            {
                this.MonthlyExpenses = new ObservableCollection<MonthlyExpense>(await StoredProcedures.GetMonthlyExpenses(this.StartDate, this.EndDate));
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void Recalculate()
        {
            this.TotalAmount = this.MonthlyExpenses.Where(expense=>expense.IsIncluded).Sum(expense => expense.Amount);            
            this.TotalSplitAmount = this.MonthlyExpenses.Where(expense => expense.IsIncluded ).Sum(expense => expense.SplitAmount);
            this.TotalOwedAmount = this.MonthlyExpenses.Where(expense => expense.IsIncluded).Sum(expense => expense.OwedAmount);
        }

        private void SaveMonthlyExpenses()
        {
            try
            {
                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    //Create MonthlyExpenseSet record
                    MonthlyExpenseSet newmonthlyexpenseset = new MonthlyExpenseSet();
                    newmonthlyexpenseset.StartDate = this.StartDate;
                    newmonthlyexpenseset.EndDate = this.EndDate;

                    context.MonthlyExpenseSets.Attach(newmonthlyexpenseset);
                    context.Entry(newmonthlyexpenseset).State = System.Data.Entity.EntityState.Added;
                    context.MonthlyExpenseSets.Add(newmonthlyexpenseset);
                    context.SaveChanges();

                    //Set Expense items SetID
                    foreach (MonthlyExpense monthlyexpense in this.MonthlyExpenses)
                    {
                        monthlyexpense.MonthlyExpenseSetID = newmonthlyexpenseset.MonthlyExpenseSetID;
                        context.MonthlyExpenses.Attach(monthlyexpense);
                        context.Entry(monthlyexpense).State = System.Data.Entity.EntityState.Added;
                        context.MonthlyExpenses.Add(monthlyexpense);
                    }

                    context.SaveChanges();

                    this.CloseAction?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
        #endregion
    }
}