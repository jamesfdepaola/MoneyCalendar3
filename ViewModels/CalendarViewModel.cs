using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using MoneyCalendar.Attributes;
using MoneyCalendar.Data;
using MoneyCalendar.DataModels;
using Prism.Commands;

namespace MoneyCalendar.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        #region Constants
        private readonly DayOfWeek _dayWeekStart = DayOfWeek.Monday;
        #endregion

        #region Members
        private DateTime _startDate;
        private DateTime _endDate;
        private int _selectedYear;
        private int _selectedMonth;
        //private bool _showDataGrid;
        private ViewState _viewState = ViewState.Calendar;
        private Transaction _selectedTransaction;
        private bool _showInactiveAccounts;
        private AccountBalance _selectedAccountDropDownListItem;
        private List<AccountBalance> _accountBalances;
        private DateTime _selectedCalenderDate;
        private bool _showAccountList;
        private bool _showTypeList;
        private AccountDatedTotals _selectedAccountTotals;
        private bool _showAllDueDates;
        private bool _selectedTransactionIsNew;
        private Transaction _copiedTransaction;
        private bool _selectedTransactionTypeIsFocused;
        private string _quickSortText;
        private List<TransactionType> _transactionTypes;
        private List<Bill> _bills;
        private List<Earner> _earners;
        private List<Job> _jobs;
        private bool _refreshingAll;
        private int _quickSortTypeID;
        #endregion

        #region Properties
        #region Data
        //public MoneyCalendarEntities MoneyContext { get; private set; }

        public List<Transaction> Transactions { get; set; }

        public DatedTransactionSetCollection DatedTransactionSets { get; set; }

        public List<TransactionType> TransactionTypes { get => this._transactionTypes; private set => this.SetProperty(ref this._transactionTypes, value); }
        public List<Bill> Bills { get => this._bills; private set => this.SetProperty(ref this._bills, value); }
        public List<Earner> Earners { get => this._earners; private set => this.SetProperty(ref this._earners, value); }
        public List<Job> Jobs { get => this._jobs; private set => this.SetProperty(ref this._jobs, value); }

        public List<AccountBalance> AccountBalances
        {
            get => this._accountBalances;
            private set
            {
                this.SetProperty(ref this._accountBalances, value);
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.AccountBalances))]
        public List<AccountBalance> AccountSelectionList { get => this._accountBalances?.Where(account => account.AccountID > 0).ToList(); }

        public Transaction CopiedTransaction { get => this._copiedTransaction; set => this.SetProperty(ref this._copiedTransaction, value); }

        [ObservesProperty(nameof(CalendarViewModel.CopiedTransaction))]
        public bool HasCopiedTransaction { get => this.CopiedTransaction != null; }
        #endregion

        #region Selections
        public DateTime SelectedCalenderDate
        {
            get => this._selectedCalenderDate;
        }
        public async Task SelectCalendarDate(DateTime value)
        {
            DateTime previousselecteddate = this.SelectedCalenderDate;

            this.SetProperty(ref this._selectedCalenderDate, value, nameof(CalendarViewModel.SelectedCalenderDate));

            if (this.SelectedTransactionIsNew)
                this.SelectedTransactionDate = this.SelectedCalenderDate;

            this.DatedTransactionSets[previousselecteddate]?.UpdateIsCalendarSelectedDate();
            this.DatedTransactionSets[this._selectedCalenderDate]?.UpdateIsCalendarSelectedDate();

            await this.GetAccountDatedTotals();
        }

        public AccountBalance SelectedAccountBalance
        {
            get => this._selectedAccountDropDownListItem;
            set
            {
                this.SetProperty(ref this._selectedAccountDropDownListItem, value, nameof(CalendarViewModel.SelectedAccountBalance));

                this.CreateCalendarTransactionList();
                Dispatcher.CurrentDispatcher.Invoke(async () => await this.GetAccountDatedTotals());
            }
        }

        public AccountDatedTotals SelectedAccountTotals { get => this._selectedAccountTotals; set => this.SetProperty(ref this._selectedAccountTotals, value); }

        public int SelectedMonth
        {
            get => this._selectedMonth;
            set
            {
                this.SetProperty(ref this._selectedMonth, value);
                this.ShowMonth();
            }
        }
        public int SelectedYear
        {
            get => this._selectedYear;
            set
            {
                this.SetProperty(ref this._selectedYear, value);
                this.ShowMonth();
            }
        }

        public int QuickSortTypeID
        {
            get => this._quickSortTypeID; 
            set
            {
                this.SetProperty(ref this._quickSortTypeID, value);
                this.CreateCalendarTransactionList();
            }
        }

        public string QuickSortText
        {
            get => this._quickSortText;
            set
            {
                this.SetProperty(ref this._quickSortText, value);
                this.CreateCalendarTransactionList();
            }
        }
        #endregion

        #region Calendar Properties
        public IEnumerable<string> WeekDays
        {
            get
            {
                for (int i = 0; i <= 6; i++)
                {
                    yield return Enum.GetName(typeof(DayOfWeek), (((int)this._dayWeekStart + i) % 7)).ToString();
                }
            }
        }

        public Dictionary<int, string> Months { get; set; }
        public List<int> Years { get; set; }

        public DateTime StartDate { get => this._startDate; }
        public void SelectStartDate(DateTime? value)
        {
            if (value != null)
            {
                //Check if date is not start of week
                if (value.Value.DayOfWeek != this._dayWeekStart)
                {
                    //Get first WeekStart before value
                    value = value.Value.AddDays(-1 * (int)(value.Value.DayOfWeek - this._dayWeekStart));
                }
                this.SetProperty(ref this._startDate, (DateTime)value, nameof(CalendarViewModel.StartDate));
                this.SetProperty(ref this._endDate, this._startDate.AddDays(34), nameof(CalendarViewModel.EndDate));
                this.CreateCalendarTransactionList();
            }
        }

        public DateTime EndDate { get => this._endDate; }
        public void SelectEndDate(DateTime value)
        {
            this.SetProperty(ref this._endDate, value, nameof(CalendarViewModel.EndDate));
            this.SetProperty(ref this._startDate, this._endDate.AddDays(-34), nameof(CalendarViewModel.StartDate));
            this.CreateCalendarTransactionList();
        }

        public bool ShowAllDueDates { get => this._showAllDueDates; set => this.SetProperty(ref this._showAllDueDates, value); }

        //public bool ShowDataGrid { get => this._showDataGrid; set => this.SetProperty(ref this._showDataGrid, value); }

        public bool ShowDataGrid 
        {
            get => this._viewState == ViewState.DataGrid;
            set
            {
                this.SetProperty(ref this._viewState, value ? ViewState.DataGrid : ViewState.Calendar);
                this.RaisePropertyChanged(nameof(this.ShowCalendar));
                this.RaisePropertyChanged(nameof(this.ShowReport));
            }
        }
        public bool ShowCalendar
        {
            get => this._viewState == ViewState.Calendar;
            set
            {
                this.SetProperty(ref this._viewState, value ? ViewState.Calendar : ViewState.DataGrid);
                this.RaisePropertyChanged(nameof(this.ShowDataGrid));
                this.RaisePropertyChanged(nameof(this.ShowReport));
            }
        }
        public bool ShowReport
        {
            get => this._viewState == ViewState.Report;
            set
            {
                this.SetProperty(ref this._viewState, value ? ViewState.Report : ViewState.Calendar);
                this.RaisePropertyChanged(nameof(this.ShowCalendar));
                this.RaisePropertyChanged(nameof(this.ShowDataGrid));
            }
        }

        public bool ShowSelectedTransactionPanel { get => this._viewState == ViewState.DataGrid || this._viewState == ViewState.Calendar; }

        public bool ShowInactiveAccounts { get => this._showInactiveAccounts; set => this.SetProperty(ref this._showInactiveAccounts, value); }

        public bool ShowSelectedTransactionTypeList { get => this._showTypeList; set => this.SetProperty(ref this._showTypeList, value); }
        public bool ShowSelectedTransactionAccountList { get => this._showAccountList; set => this.SetProperty(ref this._showAccountList, value); }
        #endregion

        #region Selected Transaction
        public Transaction SelectedTransaction
        {
            get => this._selectedTransaction;
            private set => this.SetProperty(ref this._selectedTransaction, value);
        }
        public void SelectTransaction(Transaction value)
        {
            try
            {
                this.SelectedTransactionIsNew = false;

                //save previous selected transaction
                if (this.SelectedTransaction != null)
                    this.SaveSelectedTransaction();
                //using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                //{
                //    context.Transactions.Attach(this.SelectedTransaction);
                //    context.Entry(this.SelectedTransaction).State = System.Data.Entity.EntityState.Modified;
                //    await context.SaveChangesAsync();
                //}
                //await this.MoneyContext.SaveChangesAsync();

                this.SelectedTransaction = value;

                //Dispatcher.CurrentDispatcher.Invoke(() => this.RaisePropertyChanged(nameof(CalendarViewModel.HasSelectedTransaction)));
                this.RaisePropertyChanged(nameof(CalendarViewModel.HasSelectedTransaction));

                if (this.SelectedTransaction != null)
                    this.SelectedTransaction.PropertyChanged += SelectedTransaction_PropertyChanged;
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public bool SelectedTransactionIsNew { get => this._selectedTransactionIsNew; set => this.SetProperty(ref this._selectedTransactionIsNew, value); }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public DateTime? SelectedTransactionDate
        {
            get => this.SelectedTransaction?.TransactionDate;
            set
            {
                if (value != null && this.SelectedTransaction != null)
                {
                    if (this.SelectedTransactionIsNew)
                    {
                        this.SelectedTransaction.TransactionDate = (DateTime)value;
                    }
                    else
                    {
                        DateTime previousdate = this.SelectedTransaction.TransactionDate;
                        Transaction selectedtransaction = this.SelectedTransaction;

                        selectedtransaction.TransactionDate = (DateTime)value;

                        //Refreshing the dated sets will deselect the transaction
                        this.DatedTransactionSets[previousdate].FillTransactionsSet(this.Transactions);
                        this.DatedTransactionSets[selectedtransaction.TransactionDate].FillTransactionsSet(this.Transactions);

                        //reselect transaction
                        this.DatedTransactionSets[selectedtransaction.TransactionDate].SelectedSetTransaction = selectedtransaction;
                    }

                    this.RaisePropertyChanged(nameof(this.SelectedTransactionDate));
                }
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public string SelectedTransactionDescription
        {
            get => this.SelectedTransaction?.Description;
            set
            {
                if (value != null && this.SelectedTransaction != null)
                {
                    this.SelectedTransaction.Description = value;
                    //this.RaisePropertyChanged(nameof(this.CanCancelEditingSelectedTransaction));
                }
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public decimal SelectedTransactionAmount
        {
            get => this.SelectedTransaction?.Amount ?? 0;
            set
            {
                if (this.SelectedTransaction != null)
                {
                    this.SelectedTransaction.Amount = value;
                    //this.RaisePropertyChanged(nameof(this.CanCancelEditingSelectedTransaction));
                }
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public TransactionType SelectedTransactionType
        {
            get
            {
                if (this.SelectedTransaction == null)
                    return null;
                else
                {
                    if (this.SelectedTransaction.TransactionType == null || this.SelectedTransaction.TransactionType.TypeID != this.SelectedTransaction.TypeID)
                    {
                        this.SelectedTransaction.TransactionType = this.TransactionTypes.FirstOrDefault(type => type.TypeID == this.SelectedTransaction.TypeID);
                        this.SelectedTransaction.TypeID = this.SelectedTransaction.TransactionType?.TypeID ?? 0;
                    }

                    return this.SelectedTransaction.TransactionType;
                }
            }
            set
            {
                if (this.SelectedTransaction != null)
                {
                    this.SelectedTransaction.TransactionType = value;

                    if (value != null)
                        this.SelectedTransaction.TypeID = this.SelectedTransaction.TransactionType.TypeID;

                    this.RaisePropertyChanged(nameof(CalendarViewModel.ShowSelectedTransactionTransferProperties));
                    this.RaisePropertyChanged(nameof(CalendarViewModel.SelectedTransactionTypeIsBillType));
                    this.RaisePropertyChanged(nameof(CalendarViewModel.SelectedTransactionTypeIsPaycheckType));
                }
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public bool SelectedTransactionTypeIsBillType { get => this.SelectedTransaction?.TransactionType?.IsBillType ?? false; }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public bool SelectedTransactionTypeIsPaycheckType { get => this.SelectedTransaction?.TransactionType?.IsPaycheckType ?? false; }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public bool HasSelectedTransaction { get => this.SelectedTransaction != null; }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public bool CanSaveSelectedTransaction
        {
            get
            {
                bool cansave = false;//= this.SelectedTransactionIsNew;

                if (this.SelectedTransaction != null)
                {
                    cansave = true;
                }

                //logic for specific types

                return cansave;
            }
        }

        [ObservesProperty(nameof(CalendarViewModel.SelectedTransaction))]
        public bool ShowSelectedTransactionTransferProperties
        {
            get
            {
                return this.SelectedTransaction != null && this.SelectedTransactionIsNew && this.SelectedTransactionType?.Name == "Transfer";
            }
        }

        public decimal? SelectedTransactionSettleDueTypeNewAmount { get; set; }
        public bool SelectedTransactionSettleDueTypeIsPosted { get; set; }

        [ResetOnToggle] public bool SelectedTransactionTypeIsFocused { get => this._selectedTransactionTypeIsFocused; set => this.SetProperty(ref this._selectedTransactionTypeIsFocused, value); }

        public int SelectedTransactionTransferFromAccountID { get; set; }
        public int SelectedTransactionTransferToAccountID { get; set; }
        #endregion
        #endregion

        #region Commands    
        public DelegateCommand<DateTime?> SelectStartDateCommand { get; private set; }
        public DelegateCommand RefreshAllCommand { get; private set; }

        public DelegateCommand ClearQuickSortCommand { get; private set; }

        #region Selected Transaction
        public DelegateCommand CancelChangesCommand { get; private set; }

        public DelegateCommand StartNewTransactionCommand { get; private set; }
        public DelegateCommand CancelNewTransactionCommand { get; private set; }

        public DelegateCommand SaveSelectedTransactionCommand { get; private set; }
        public DelegateCommand<DateTime?> MoveSelectedTransactionCommand { get; private set; }
        public DelegateCommand CutSelectedTransactionCommand { get; private set; }
        public DelegateCommand CopySelectedTransactionCommand { get; private set; }
        public DelegateCommand PasteSelectedTransactionCommand { get; private set; }
        public DelegateCommand DeleteSelectedTransactionCommand { get; private set; }

        public DelegateCommand SelectedTransactionPostCommand { get; private set; }
        public DelegateCommand SelectedTransactionSettleDueTypeCommand { get; private set; }
        public DelegateCommand<int?> SelectedTransactionMoveToAccountCommand { get; private set; }

        public DelegateCommand<DateTime?> DateSelectedCommand { get; private set; }

        public DelegateCommand<Transaction> TransactionSelectingCommand { get; private set; }
        public DelegateCommand<Transaction> TransactionSelectedCommand { get; private set; }
        #endregion

        private void SetupCommands()
        {
            this.ClearQuickSortCommand = new DelegateCommand(() =>
            {
                this._quickSortTypeID = -1;
                this._quickSortText = null;

                this.RaisePropertyChanged(nameof(this.QuickSortTypeID));
                this.RaisePropertyChanged(nameof(this.QuickSortText));
                this.CreateCalendarTransactionList();
            });

            this.RefreshAllCommand = new DelegateCommand(async () => await this.RefreshAll());

            this.SelectStartDateCommand = new DelegateCommand<DateTime?>(this.SelectStartDate);

            this.CancelChangesCommand = new DelegateCommand(this.CancelChanges);//.ObservesCanExecute(() => this.CanCancelEditingSelectedTransaction);

            this.SaveSelectedTransactionCommand = new DelegateCommand(async () =>
            {
                this.SaveSelectedTransaction();
                this.DatedTransactionSets[this.SelectedTransaction.TransactionDate].FillTransactionsSet(this.Transactions);
                await this.GetAccountDatedTotals();
                //await this.Refresh();
            }).ObservesCanExecute(() => this.CanSaveSelectedTransaction);

            this.MoveSelectedTransactionCommand = new DelegateCommand<DateTime?>(this.MoveSelectedTransaction);
            this.CutSelectedTransactionCommand = new DelegateCommand(this.CutSelectedTransaction);
            this.CopySelectedTransactionCommand = new DelegateCommand(this.CopySelectedTransaction);
            this.PasteSelectedTransactionCommand = new DelegateCommand(this.PasteSelectedTransaction).ObservesCanExecute(() => this.HasCopiedTransaction);
            this.DeleteSelectedTransactionCommand = new DelegateCommand(this.DeleteSelectedTransaction).ObservesCanExecute(() => this.HasSelectedTransaction);

            this.SelectedTransactionPostCommand = new DelegateCommand(async () => await this.SelectedTransactionPost());
            this.SelectedTransactionSettleDueTypeCommand = new DelegateCommand(async () => await this.SelectedTransactionSettleDueType());
            this.SelectedTransactionMoveToAccountCommand = new DelegateCommand<int?>(async (newaccountid) => await this.SelectedTransactionMoveToAccount(newaccountid));

            this.StartNewTransactionCommand = new DelegateCommand(this.StartNewTransaction);
            this.CancelNewTransactionCommand = new DelegateCommand(() => this.SelectTransaction(null));

            this.DateSelectedCommand = new DelegateCommand<DateTime?>(this.DateSelected);

            this.TransactionSelectingCommand = new DelegateCommand<Transaction>(this.TransactionSelecting);
            this.TransactionSelectedCommand = new DelegateCommand<Transaction>(this.SelectTransaction);
        }
        #endregion

        #region Class Methods
        public CalendarViewModel() : base()      //MoneyCalendarEntities moneycontext) : base()
        {
            try
            {
                //this.MoneyContext = moneycontext;

                this.DatedTransactionSets = new DatedTransactionSetCollection();

                this.Months = new Dictionary<int, string>();
                for (int m = 1; m <= 12; m++)
                {
                    this.Months.Add(m, new DateTime(1982, m, 1).ToString("MMMM"));
                }

                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    this.Years = context.Transactions.Select(transaction => transaction.TransactionDate.Year).Distinct().OrderByDescending(year => year).ToList();
                }

                this.ShowAllDueDates = true;

                this.SetupCommands();

                Dispatcher.CurrentDispatcher.Invoke(this.RefreshAll);
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private async Task LoadLists()
        {
            try
            {
                this.AccountBalances = await StoredProcedures.GetAccountsBalances();
                //await this.SelectAccountBalance(this.AccountBalances.FirstOrDefault(account => account.IsDefaultOpen));
                //this.SelectedAccountBalance = this.AccountBalances.FirstOrDefault(account => account.IsDefaultOpen);

                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    this.TransactionTypes = context.TransactionTypes.Where(type => type.IsActive && type.IsMoneyType).OrderBy(type => type.Name).ToList();
                    this.Bills = context.Bills.Where(bill => bill.IsActive).OrderBy(bill => bill.Name).ToList();
                    this.Earners = context.Earners.Where(earner => earner.IsActive).OrderBy(earner => earner.Name).ToList();
                    this.Jobs = context.Jobs.Where(job => job.IsActive).OrderBy(job => job.Employer).ToList();
                }

                this.SelectedAccountBalance = this.AccountBalances.FirstOrDefault(account => account.AccountID == (MoneyApplication.DbSettings.DefaultAccountID ?? 0));
                if (this.SelectedAccountBalance == null)
                {
                    this.SelectedAccountBalance = this.AccountBalances.FirstOrDefault(account => account.AccountID > 0);
                    if (this.SelectedAccountBalance != null)
                    {
                        MoneyApplication.DbSettings.DefaultAccountID = this.SelectedAccountBalance.AccountID;
                        MoneyApplication.SaveDbSetting(nameof(DbSettings.DefaultAccountID), MoneyApplication.DbSettings.DefaultAccountID);
                    }
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        public void Close()
        {
            this.CancelChanges();
            //this.SaveSelectedTransaction();

            //if (this.MoneyContext != null)
            //{
            //    try
            //    {
            //        this.MoneyContext.SaveChanges();

            //        if (this.MoneyContext.Database.Connection.State == System.Data.ConnectionState.Open)
            //            this.MoneyContext.Database.Connection.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        MoneyApplication.ErrorHandler(ex);
            //    }
            //    finally
            //    {
            //        this.MoneyContext.Dispose();
            //        this.MoneyContext = null;
            //    }
            //}
        }
        #endregion

        #region Date Navigation
        private async Task ShowMonth(DateTime? monthdate = null)
        {
            DateTime date;
            if (monthdate == null)
                date = new DateTime(this.SelectedYear, this.SelectedMonth, 1);
            else
                date = (DateTime)monthdate;

            try
            {
                //check if viewing a calendar
                if (this.ShowDataGrid)
                {
                    //calculate range based on just the passed month
                    this._startDate = date.AddDays(-(date.Day - 1));
                    this._endDate = this._startDate.AddDays(DateTime.DaysInMonth(this._startDate.Year, this._startDate.Month) - 1);
                }
                else
                {
                    //Calculate current month's start date
                    this._startDate = date.AddDays(-(date.Day - 1));

                    //calculate the date of the "week start" before the first date of the month
                    this._startDate = this._startDate.AddDays(this._dayWeekStart - this._startDate.DayOfWeek);

                    //end the range after 34 days to show in 35 day calendar
                    this._endDate = this._startDate.AddDays(34);

                    this._selectedMonth = date.Month;
                    this.RaisePropertyChanged(nameof(this.SelectedMonth));
                    this._selectedYear = date.Year;
                    this.RaisePropertyChanged(nameof(this.SelectedYear));
                }

                this.CreateCalendarTransactionList();
                await this.SelectCalendarDate(DateTime.Today);
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex, "Error Showing Month.");
            }
        }

        private void DateSelected(DateTime? date)
        {
            System.Windows.MessageBox.Show(date?.ToString() ?? "null");
        }

        private void TransactionSelecting(Transaction transaction)
        {
            foreach (DatedTransactionSet datedtransactionset in this.DatedTransactionSets)
            {
                if (transaction == null || datedtransactionset.SetDate != transaction.TransactionDate)
                {
                    datedtransactionset.ClearSelection();
                }
            }
        }
        #endregion

        #region Data
        public async Task Refresh()
        {
            int? transactionid = this.SelectedTransaction?.TransactionID;

            this.CreateCalendarTransactionList();
            await this.GetAccountDatedTotals();

            if (transactionid != null)
                this.SelectTransaction(this.Transactions.FirstOrDefault(transaction => transaction.TransactionID == transactionid));
        }

        public async Task RefreshAll()
        {
            try
            {
                this._refreshingAll = true;

                await this.Refresh();

                await this.LoadLists();

                await this.ShowMonth(DateTime.Today);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }
            finally
            {
                this._refreshingAll = false;

                this.CreateCalendarTransactionList();
            }
        }

        public void CreateCalendarTransactionList()
        {
            if (this._refreshingAll)
                return;

            //clear all transactions
            this.DatedTransactionSets.Clear();
            this.SelectTransaction(null);

            Expression<Func<Transaction, bool>> accountfilter = null;
            Func<Transaction, bool> typefilter = null;
            Func<Transaction, bool> textfilter = null;

            //Get the transactions
            try
            {
                ParameterExpression transactionparameter = System.Linq.Expressions.Expression.Parameter(typeof(Transaction), "transaction");

                //Account Filter
                Expression expression = null;
                if (this.SelectedAccountBalance?.IsGroupSelection ?? false)
                {
                    expression = Expression.Property(transactionparameter, nameof(Transaction.Account));
                    expression = Expression.Property(expression, nameof(Account.AccountTypeID));
                    expression = Expression.Equal(expression, Expression.Constant(this.SelectedAccountBalance.AccountTypeID));
                }
                else
                {
                    expression = Expression.Property(transactionparameter, nameof(Transaction.AccountID));
                    expression = Expression.Equal(expression, Expression.Constant(this.SelectedAccountBalance?.AccountID ?? -1));
                }

                if (this.ShowAllDueDates)
                {
                    Expression duedatefilter = Expression.Property(transactionparameter, nameof(Transaction.TransactionType));
                    duedatefilter = Expression.Property(duedatefilter, nameof(TransactionType.IsDueType));
                    expression = Expression.OrElse(duedatefilter, expression);
                }

                accountfilter = Expression.Lambda<Func<Transaction, bool>>(expression, transactionparameter);

                //Add description filter
                if (!string.IsNullOrEmpty(this.QuickSortText))
                    textfilter = (transaction) => (!string.IsNullOrEmpty(transaction.Venue) && transaction.Venue.ToUpper().Contains(this.QuickSortText.ToUpper()))
                              || (!string.IsNullOrEmpty(transaction.Description) && transaction.Description.ToUpper().Contains(this.QuickSortText.ToUpper()))
                              || (!string.IsNullOrEmpty(transaction.Bill?.CompanyName) && transaction.Bill.CompanyName.ToUpper().Contains(this.QuickSortText.ToUpper()));
                else
                    textfilter = (transaction) => true;

                //Add type filter
                if (this.QuickSortTypeID > 0)
                    typefilter = (transaction) => transaction.TypeID == this.QuickSortTypeID;
                else
                    typefilter = (transaction) => true;

                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    this.Transactions = context
                        .Transactions
                        .Include(nameof(Transaction.Account)).Include(nameof(Transaction.TransactionType)).Include(nameof(Transaction.DueType)).Include(nameof(Transaction.TransferCoTransaction))
                        .Include(nameof(Transaction.Bill)).Include(nameof(Transaction.Job)).Include(nameof(Transaction.Earner))
                        .Where(transaction => transaction.TransactionDate >= this.StartDate && transaction.TransactionDate <= this.EndDate)
                        .Where(accountfilter)
                        .Where(textfilter)
                        .Where(typefilter)
                        .OrderBy(transaction => transaction.TransactionDate).ToList();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex, "Error creating transaction list.");
            }

            try
            {
                //Fill the day collections
                for (int i = 0; i < (this.EndDate - this.StartDate).Days + 1; i++)
                {
                    this.DatedTransactionSets.Add(new DatedTransactionSet(this, this.StartDate.AddDays(i)));
                }

                this.SetProperty(ref this._selectedMonth, this.DatedTransactionSets[17].SetDate.Month, nameof(CalendarViewModel.SelectedMonth));
                this.SetProperty(ref this._selectedYear, this.DatedTransactionSets[17].SetDate.Year, nameof(CalendarViewModel.SelectedYear));
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private async Task GetAccountDatedTotals()
        {
            if (this.SelectedCalenderDate != DateTime.MinValue)
                this.SelectedAccountTotals = await StoredProcedures.GetAccountTotals(this.SelectedCalenderDate, (int)(this.SelectedAccountBalance?.AccountID ?? -1));

            //Update the Balance on the account selection list
            if (this.SelectedAccountTotals != null && this.SelectedAccountBalance != null)
            {
                this.SelectedAccountBalance.Balance = this.SelectedAccountTotals.CurrentBalance;
                this.RaisePropertyChanged(nameof(CalendarViewModel.SelectedAccountBalance));
            }
        }

        private void CancelChanges()
        {
            if (this.SelectedTransactionIsNew)
                this.SelectedTransaction = null;
            else
                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    //System.Diagnostics.Debugger.Break();
                    //    this.MoneyContext.Entry(this.SelectedTransaction).CurrentValues.SetValues(this.MoneyContext.Entry(this.SelectedTransaction).OriginalValues);
                    //this.MoneyContext.Entry(this.SelectedTransaction).State = System.Data.Entity.EntityState.Unchanged;
                }

            this.RaisePropertyChanged(nameof(this.SelectedTransaction));
            this.RaisePropertyChanged(nameof(this.SelectedTransactionDate));
            this.RaisePropertyChanged(nameof(this.SelectedTransactionDescription));
            //this.RaisePropertyChanged(nameof(this.CanCancelEditingSelectedTransaction));
        }

        #region Selected Transaction
        private async Task SelectedTransactionPost()
        {
            if (this.SelectedTransaction != null)
            {
                this.SelectedTransaction.IsCompleted = !this.SelectedTransaction.IsCompleted;
                this.SaveSelectedTransaction();

                await this.Refresh();
            }
        }

        private async Task SelectedTransactionSettleDueType()
        {
            if (this.SelectedTransaction != null)
            {
                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    context.SettleDueType(this.SelectedTransaction, this.SelectedTransactionSettleDueTypeNewAmount, this.SelectedTransactionSettleDueTypeIsPosted);
                }

                this.RaisePropertyChanged(nameof(CalendarViewModel.SelectedTransaction));

                this.SelectedTransactionSettleDueTypeNewAmount = null;
                this.SelectedTransactionSettleDueTypeIsPosted = false;

                this.SaveSelectedTransaction();
                await this.Refresh();
            }
        }

        private async Task SelectedTransactionMoveToAccount(int? newaccountid)
        {
            try
            {
                if (newaccountid != null)
                {
                    this.SelectedTransaction.AccountID = (int)newaccountid;
                    this.SelectedTransaction.Account = null;

                    this.SaveSelectedTransaction();
                    await this.Refresh();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void SelectedTransaction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.SelectedTransaction));
        }

        private void MoveSelectedTransaction(DateTime? movetodate)
        {
            if (this.SelectedTransaction != null && movetodate != null)
            {
                //Get date to refresh after the move
                DateTime datefrom = this.SelectedTransaction.TransactionDate;
                DateTime dateto = (DateTime)movetodate;

                this.SelectedTransaction.TransactionDate = dateto;
                this.SaveSelectedTransaction();

                this.DatedTransactionSets[datefrom].FillTransactionsSet(this.Transactions);
                this.DatedTransactionSets[dateto].FillTransactionsSet(this.Transactions);
            }
        }

        private void CutSelectedTransaction()
        {
            this.CopiedTransaction = this.SelectedTransaction;

            this.DeleteSelectedTransaction();
        }

        private void CopySelectedTransaction()
        {
            this.CopiedTransaction = this.SelectedTransaction;
        }

        private void PasteSelectedTransaction()
        {
            try
            {
                Transaction copy = this.CopiedTransaction.Clone();
                copy.TransactionDate = this.SelectedCalenderDate;
                this.Transactions.Add(copy);
                this.DatedTransactionSets[this.SelectedCalenderDate].FillTransactionsSet(this.Transactions);
                this.SelectTransaction(copy);
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private int? SaveSelectedTransaction()
        {
            int? transactionid = null;
            Transaction fromtransaction = null;

            try
            {
                if (this.SelectedTransaction != null)
                {
                    using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                    {
                        Transaction dbtransaction = context.Transactions.FirstOrDefault(transaction => transaction.TransactionID == this.SelectedTransaction.TransactionID);

                        //Check if this is a new transaction
                        if (dbtransaction == null)
                        {
                            //Check if creating a Transfer
                            if (this.ShowSelectedTransactionTransferProperties)
                            {
                                //Switch account id 
                                this.SelectedTransaction.AccountID = this.SelectedTransactionTransferToAccountID;

                                //Create From transaction
                                fromtransaction = this.SelectedTransaction.Clone();
                                fromtransaction.AccountID = this.SelectedTransactionTransferFromAccountID;
                                fromtransaction.IsTransferFrom = true;
                                fromtransaction.Amount *= -1;

                                context.Transactions.Attach(fromtransaction);
                                context.Entry(fromtransaction).State = System.Data.Entity.EntityState.Added;

                                //Add only the transaction for the current account
                                if (this.SelectedTransaction.AccountID == this.SelectedAccountBalance.AccountID)
                                    this.Transactions.Add(this.SelectedTransaction);
                                else if (fromtransaction.AccountID == this.SelectedAccountBalance.AccountID)
                                    this.Transactions.Add(fromtransaction);
                            }
                            else
                                this.Transactions.Add(this.SelectedTransaction);

                            context.Transactions.Attach(this.SelectedTransaction);
                            context.Entry(this.SelectedTransaction).State = System.Data.Entity.EntityState.Added;
                        }
                        else
                        {
                            context.Entry(dbtransaction).CurrentValues.SetValues(this.SelectedTransaction);
                            context.Entry(dbtransaction).State = System.Data.Entity.EntityState.Modified;
                        }

                        context.SaveChanges();
                        transactionid = this.SelectedTransaction.TransactionID;

                        if (fromtransaction != null)
                        {
                            //Update their CoTransactionIDs
                            this.SelectedTransaction.TransferCoTransactionID = fromtransaction.TransactionID;
                            fromtransaction.TransferCoTransactionID = transactionid;

                            context.SaveChanges();
                        }

                        this.SelectedTransactionIsNew = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return transactionid;
        }

        private void DeleteSelectedTransaction()
        {
            //Check if merely need to clear new trasnaction
            if (this.SelectedTransactionIsNew)
            {
                this.SelectTransaction(null);
            }
            else
            {
                try
                {
                    //Move record into deleted transaction
                    DeletedTransaction deletedtransaction = this.SelectedTransaction.CopyProperties<Transaction, DeletedTransaction>(includekeys: true);

                    using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                    {
                        context.DeletedTransactions.Add(deletedtransaction);
                        context.Entry(deletedtransaction).State = System.Data.Entity.EntityState.Added;

                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    MoneyApplication.ErrorHandler(ex);
                }

                try
                {
                    using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                    {
                        context.Transactions.Remove(
                        context.Transactions.First(transaction => transaction.TransactionID == this.SelectedTransaction.TransactionID));

                        context.SaveChanges();

                        this.SelectedTransaction = null;
                    }

                    this.CreateCalendarTransactionList();
                }
                catch (Exception ex)
                {
                    MoneyApplication.ErrorHandler(ex);
                }
            }
        }
        #endregion

        public void StartNewTransaction()
        {
            this.StartNewTransaction(new Transaction() { TransactionDate = this.SelectedCalenderDate });
        }
        public void StartNewTransaction(TransactionType transactiontype)
        {
            this.StartNewTransaction(new Transaction() { TransactionDate = this.SelectedCalenderDate, TypeID = transactiontype.TypeID, TransactionType = transactiontype });
        }
        private void StartNewTransaction(Transaction transaction)
        {
            this.CancelChanges();

            this.SelectTransaction(transaction);

            if (this.SelectedAccountBalance.AccountID == null)
                this.ShowSelectedTransactionAccountList = true;
            else
            {
                this.SelectedTransaction.AccountID = (int)this.SelectedAccountBalance.AccountID;
                this.ShowSelectedTransactionTypeList = this.SelectedTransactionTypeIsFocused = true;
            }

            this.SelectedTransactionIsNew = true;
        }
        #endregion
    }

    public class TransactionSelectEventArgs : EventArgs
    {
        public Transaction Transaction { get; private set; }

        public TransactionSelectEventArgs(Transaction transaction)
        {
            this.Transaction = transaction;
        }
    }

    public enum ViewState
    {
        Calendar,
        DataGrid,
        Report
    }

    public class DatedTransactionSet : BindableBasePlus
    {
        #region Members
        private DateTime _setDate;
        private Transaction _selectedTransaction;
        private bool _isCalendarSelectedDate;
        #endregion

        #region Properties
        public CalendarViewModel CalendarViewModel { get; private set; }

        public bool IsCalendarSelectedDate { get => this.SetDate == this.CalendarViewModel.SelectedCalenderDate; }

        public DateTime SetDate { get => this._setDate; set => this.SetProperty(ref this._setDate, value); }

        public ObservableCollection<Transaction> SetTransactions { get; set; }

        public Transaction SelectedSetTransaction
        {
            get => this._selectedTransaction;
            set
            {
                this.CalendarViewModel.TransactionSelectingCommand?.Execute(value);

                this.SetProperty(ref this._selectedTransaction, value);

                this.CalendarViewModel.TransactionSelectedCommand?.Execute(this.SelectedSetTransaction);
            }
        }

        [ObservesProperty(nameof(DatedTransactionSet.SelectedSetTransaction))]
        public bool HasSelectedSetTransaction { get => this.SelectedSetTransaction != null; }
        #endregion

        #region Commands
        public DelegateCommand SelectDatedTransactionSetCommand { get; set; }

        public DelegateCommand StartNewTransactionCommand { get; set; }
        public DelegateCommand SaveSelectedSetTransactionCommand { get; set; }
        public DelegateCommand DeleteSelectedSetTransactionCommand { get; set; }

        public DelegateCommand TesterCommand { get; set; } = new DelegateCommand(() => System.Windows.MessageBox.Show("test!"));
        #endregion

        #region Class Methods
        public DatedTransactionSet(CalendarViewModel calendarviewmodel, DateTime date)
        {
            this.SetTransactions = new ObservableCollection<Transaction>();
            this.CalendarViewModel = calendarviewmodel;
            this.SetDate = date;

            this.FillTransactionsSet(this.CalendarViewModel.Transactions);

            this.SelectDatedTransactionSetCommand = new DelegateCommand(async () => await this.CalendarViewModel.SelectCalendarDate(this.SetDate));

            this.StartNewTransactionCommand = new DelegateCommand(() => this.CalendarViewModel.StartNewTransactionCommand.Execute());
            this.SaveSelectedSetTransactionCommand = new DelegateCommand(() => this.CalendarViewModel.SaveSelectedTransactionCommand.Execute());
            this.DeleteSelectedSetTransactionCommand = new DelegateCommand(() => this.CalendarViewModel.DeleteSelectedTransactionCommand.Execute());


        }
        
        public void FillTransactionsSet(List<Transaction> transactions)
        {
            this.SetTransactions.Clear();

            this.SetTransactions.AddRange(transactions.Where(transaction => transaction.TransactionDate == this.SetDate));
        }

        public void ClearSelection()
        {
            this._selectedTransaction = null;
            this.RaisePropertyChanged(nameof(SelectedSetTransaction));
            this.RaisePropertyChanged(nameof(HasSelectedSetTransaction));
        }

        public void UpdateIsCalendarSelectedDate()
        {
            //this._isCalendarSelectedDate = (this.SetDate == this.CalendarViewModel.SelectedCalenderDate);
            this.RaisePropertyChanged(nameof(this.IsCalendarSelectedDate));
        }
        #endregion
    }

    public class DatedTransactionSetCollection : ObservableCollection<DatedTransactionSet>
    {
        public DatedTransactionSet this[DateTime date]
        {
            get => this.Items.FirstOrDefault(set => set.SetDate == date);
        }
    }
}