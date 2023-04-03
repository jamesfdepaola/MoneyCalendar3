using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MoneyCalendar.Data;
using MoneyCalendar.DataModels;
using Prism.Commands;

namespace MoneyCalendar.ViewModels
{
    public class LoadDueDatesViewModel : ViewModelBase
    {
        #region Members
        private ObservableCollection<DueTransaction> _dueTransactions;
        private Window _window = null;

        private int _year;
        private int _month;

        private bool _includeAllBills = false;
        private bool _includeAllPaychecks = false;
        #endregion

        #region Properties
        public CalendarViewModel CalendarViewModel { get; private set; }

        public ObservableCollection<DueTransaction> DueTransactions { get => this._dueTransactions; private set => this.SetProperty(ref this._dueTransactions, value); }

        public List<DueTransaction> DueBills { get => this._dueTransactions?.Where(due=>due.BillID != null).ToList(); }
        public List<DueTransaction> DuePaychecks { get => this._dueTransactions?.Where(due => due.JobID != null).ToList(); }

        public int Year
        {
            get => this._year;
            set
            {
                this.SetProperty(ref this._year, value);
                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadDueTransactions());
            }
        }
        public int Month
        {
            get => this._month; set
            {
                this.SetProperty(ref this._month, value);
                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadDueTransactions());
            }
        }

        public bool IncludeAllBills
        {
            get => this._includeAllBills;
            set
            {
                this.SetProperty(ref this._includeAllBills, value);

                foreach (DueTransaction duetransaction in this.DueBills)
                {
                    duetransaction.Include = this.IncludeAllBills;
                }

                this.RaiseListsChanged();
            }
        }

        public bool IncludeAllPaychecks
        {
            get => this._includeAllPaychecks;
            set
            {
                this.SetProperty(ref this._includeAllPaychecks, value);

                foreach (DueTransaction duetransaction in this.DuePaychecks)
                {
                    duetransaction.Include = this._includeAllPaychecks;
                }

                this.RaiseListsChanged();
            }
        }

        #endregion

        #region Commands    
        public DelegateCommand CreateTransactionsCommand { get; set; }
        #endregion

        #region Class Methods
        public LoadDueDatesViewModel(Window window, CalendarViewModel calendarviewmodel) : base()
        {
            try
            {
                this._window = window;
                this.CalendarViewModel = calendarviewmodel;

                this._year = this.CalendarViewModel.SelectedYear;
                this._month = this.CalendarViewModel.SelectedMonth;

                this.SetupCommands();

                Dispatcher.CurrentDispatcher.Invoke(() => this.LoadDueTransactions());
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void SetupCommands()
        {
            this.CreateTransactionsCommand = new DelegateCommand(async () => await this.CreateDueTransactions());
        }

        private void RaiseListsChanged()
        {
            this.RaisePropertyChanged(nameof(LoadDueDatesViewModel.DueBills));
            this.RaisePropertyChanged(nameof(LoadDueDatesViewModel.DuePaychecks));
        }
        #endregion

        #region Data
        public async Task LoadDueTransactions()
        {
            try
            {
                this.DueTransactions = new ObservableCollection<DueTransaction>(await StoredProcedures.GetDueDates(this.Year, this.Month));

                this.RaiseListsChanged();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        public async Task CreateDueTransactions()
        {
            using (MoneyCalendarEntities context = new MoneyCalendarEntities())
            {
                foreach (DueTransaction duedate in this.DueTransactions.Where(due=>due.Include))
                {
                    Transaction newtransaction = duedate.CopyProperties<DueTransaction, Transaction>();

                    if (duedate.BillID != null)
                        newtransaction.DueAmount *= -1;

                    context.Transactions.Add(newtransaction);
                    context.Entry(newtransaction).State = System.Data.Entity.EntityState.Added;
                }

                context.SaveChanges();
            }

            await this.CalendarViewModel.Refresh();
            this._window.Close();
        }
        #endregion
    }
}