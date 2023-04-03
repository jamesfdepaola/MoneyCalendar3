using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using MoneyCalendar.DataModels;
using Prism.Commands;

namespace MoneyCalendar.ViewModels
{
    public class BillsViewModel : ViewModelBase
    {
        #region Members
        private ObservableCollection<Bill> _bills;

        private MoneyCalendarEntities _context;
        private Bill _selectedBill;
        #endregion

        #region Properties
        public CalendarViewModel CalendarViewModel { get; private set; }

        public ObservableCollection<Bill> Bills { get => this._bills; private set => this.SetProperty(ref this._bills, value); }

        public Bill SelectedBill { get => this._selectedBill; set => this.SetProperty(ref this._selectedBill ,value); }

        public List<AccountType> AccountTypes { get; private set; }

        public bool ShowInactive { get; set; }
        #endregion

        #region Commands    
        public DelegateCommand SaveBillsCommand { get; set; }
        public DelegateCommand RefreshBillsCommand { get; set; }
        #endregion

        #region Class Methods
        public BillsViewModel(CalendarViewModel calendarviewmodel) : base()
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
            this.RefreshBillsCommand = new DelegateCommand(async () => await this.LoadBills());
            this.SaveBillsCommand = new DelegateCommand(this.SaveBills);
        }
        #endregion

        #region Data
        private async Task RefreshAll()
        {
            await this.LoadBills();
        }

        public async Task LoadBills()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                this.Bills = new ObservableCollection<Bill>(await this._context
                    .Bills
                    .Include(bill => bill.Transactions)
                    .Where(bill => this.ShowInactive || bill.IsActive)
                    .OrderByDescending(bill => bill.IsActive)
                    .ThenBy(bill => bill.Name)
                    .ToListAsync());
                this.Bills.CollectionChanged += Bills_CollectionChanged;

                this.Bills.ToList().ForEach((bill) => bill.Transactions = bill.Transactions.OrderByDescending((transaction) => transaction.TransactionDate).ToList());
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void Bills_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Bill bill in e.NewItems)
            {
                this._context.Bills.Attach(bill);
                this._context.Entry(bill).State = EntityState.Added;
            }
        }

        private void SaveBills()
        {
            this._context.SaveChanges();
        }
        #endregion
    }
}