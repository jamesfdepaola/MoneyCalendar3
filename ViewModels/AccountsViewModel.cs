using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MoneyCalendar.DataModels;
using Prism.Commands;

namespace MoneyCalendar.ViewModels
{
    public class AccountsViewModel : ViewModelBase
    {
        #region Members
        private ObservableCollection<Account> _accounts;

        private MoneyCalendarEntities _context;
        #endregion

        #region Properties
        public CalendarViewModel CalendarViewModel { get; private set; }        

        public ObservableCollection<Account> Accounts { get => this._accounts; private set => this.SetProperty(ref this._accounts, value); }

        public List<AccountType> AccountTypes { get; private set; }

        public bool ShowInactive { get; set; }

        public DbSettings DbSettings { get; set; }
        public int? DefaultAccountID 
        {
            get => this.DbSettings.DefaultAccountID;
            set
            {
                this.DbSettings.DefaultAccountID = value;

                MoneyApplication.SaveDbSetting(nameof(DbSettings.DefaultAccountID), value);
            }
        }
        #endregion

        #region Commands    
        public DelegateCommand RefreshAccountsCommand { get; set; }
        #endregion

        #region Class Methods
        public AccountsViewModel(CalendarViewModel calendarviewmodel) : base()
        {
            try
            {
                this.CalendarViewModel = calendarviewmodel;

                this._context = new MoneyCalendarEntities();

                this.SetupCommands();

                this.RefreshAll();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void SetupCommands()
        {
            this.RefreshAccountsCommand = new DelegateCommand(async () => await this.LoadAccounts());
            this.SaveAccountsCommand = new DelegateCommand(this.SaveAccounts);
        }
        #endregion

        #region Data
        private async Task RefreshAll()
        {
            await this.LoadAccountTypes();
            await this.LoadAccounts();
        }

        private async Task LoadAccountTypes()
        {
            try
            {
                //using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                //{
                    this.AccountTypes = await _context.AccountTypes.OrderBy(type => type.Sort).ToListAsync();
                //}
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        public async Task LoadAccounts()
        {
            try
            {
                //using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                //{
                    this.Accounts = new ObservableCollection<Account>(await this._context
                    .Accounts.Include(nameof(Account.AccountType))
                    .Where(account => account.AccountType.IsCashType || account.AccountType.IsCreditType)
                    .Where(account => this.ShowInactive || account.IsActive)
                    .OrderBy(account => account.Sort)
                    .ToListAsync());
                this.Accounts.CollectionChanged += Accounts_CollectionChanged;
                //}
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void Accounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debugger.Break();

            foreach (Account account in e.NewItems)
            {
                this._context.Accounts.Attach(account);
                this._context.Entry(account).State = EntityState.Added;
            }
        }

        public DelegateCommand SaveAccountsCommand { get; set; }

        private void SaveAccounts()
        {
            
            this._context.SaveChanges();
        }
        #endregion
    }
}