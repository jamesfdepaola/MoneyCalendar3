using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using MoneyCalendar.Attributes;
using MoneyCalendar.Data;
using MoneyCalendar.DataModels;
using Prism.Commands;

namespace MoneyCalendar.ViewModels
{
    public class NewTransactionBatchViewModel : ViewModelBase
    {
        #region Members
        private bool _newEntryDateIsFocused;
        private bool _newEntryTypeIsFocused;
        private bool _newEntryVenueIsFocused;
        private bool _newEntryDescriptionIsFocused;

        private bool _showTypeList;

        private bool _showVenueAutoCompleteSuggestions;
        private bool _showDescriptionAutoCompleteSuggestions;
        
        private int _newEntryVenueSuggestionIndex;

        private List<string> _newEntryVenueSuggestions;
        private List<string> _newEntryDescriptionSuggestions;
        private bool _newEntryAccountIsFocused;
        #endregion

        #region Properties
        public CalendarViewModel CalendarViewModel { get; private set; }

        //public MoneyCalendarEntities MoneyContext { get=>this.CalendarViewModel.MoneyContext; }

        public ObservableCollection<Transaction> NewTransactionBatch { get; set; }

        public Transaction NewEntry{ get; set; }

        [ResetOnToggle] public bool NewEntryDateIsFocused { get=>this._newEntryDateIsFocused; set=>this.SetProperty(ref this._newEntryDateIsFocused, value); }        
        [ResetOnToggle] public bool NewEntryTypeIsFocused { get => this._newEntryTypeIsFocused; set => this.SetProperty(ref this._newEntryTypeIsFocused, value); }
        [ResetOnToggle] public bool NewEntryVenueIsFocused { get => this._newEntryVenueIsFocused; set => this.SetProperty(ref this._newEntryVenueIsFocused, value); }
        [ResetOnToggle] public bool NewEntryDescriptionIsFocused { get => this._newEntryDescriptionIsFocused; set => this.SetProperty(ref this._newEntryDescriptionIsFocused, value); }
        [ResetOnToggle] public bool NewEntryAccountIsFocused { get => this._newEntryAccountIsFocused; set => this.SetProperty(ref this._newEntryAccountIsFocused, value); }
        
        public bool ShowNewEntryTypeList
        {
            get => this._showTypeList;
            set
            {
                this.SetProperty(ref this._showTypeList, value);
                if (this.ShowNewEntryTypeList)
                {
                    this.NewEntryTypeIsFocused = false;
                    this.NewEntryTypeIsFocused = true;
                }
            }
        }

        public string NewEntryVenue
        {
            get => this.NewEntry?.Venue;
            set
            {
                this.NewEntry.Venue = value;

                //get autocomplete
                this.StartGetAutoCompleteListTask(() => this.NewEntryVenueSuggestions, () =>
                  {
                      //TransactionBatchNewEntryVenueSuggestions = this.MoneyContext
                      using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                      {
                          return context.Transactions
                              .Where(transaction => transaction.Venue.Contains(this.NewEntryVenue))
                              .Select(transaction => transaction.Venue).Distinct()
                              .OrderBy(venue => venue)
                              .ToList();
                      }
                  });
            }
        }
        public string NewEntryDescription
        {
            get => this.NewEntry?.Description;
            set
            {
                this.NewEntry.Description = value;

                //get autocomplete
                this.StartGetAutoCompleteListTask(() => this.NewEntryDescriptionSuggestions, () =>
                {
                    using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                    {
                        //TransactionBatchNewEntryVenueSuggestions = this.MoneyContext
                        return context.Transactions
                            .Where(transaction => transaction.Description.Contains(this.NewEntryDescription))
                            .Select(transaction => transaction.Description).Distinct()
                            .OrderBy(description => description)
                            .ToList();
                    }
                });
            }
        }

        [ObservesProperty(nameof(NewTransactionBatchViewModel.NewEntryVenueSuggestions))]
        [ObservesProperty(nameof(NewTransactionBatchViewModel.NewEntryDescriptionSuggestions))]
        public List<string> AutoCompleteSuggestions
        {
            get
            {
                List<string> suggestions = null;

                if (this.ShowVenueAutoCompleteSuggestions)
                {
                    suggestions = this.NewEntryVenueSuggestions;
                }
                else if (this.ShowDescriptionAutoCompleteSuggestions)
                    suggestions = this.NewEntryDescriptionSuggestions;

                return suggestions;
            }
        }
        public List<string> NewEntryVenueSuggestions 
        { 
            get=>this._newEntryVenueSuggestions;
            private set
            {
                this.SetProperty(ref this._newEntryVenueSuggestions, value);

                if (this._newEntryVenueSuggestions.Count > 0)
                {
                    this.ShowVenueAutoCompleteSuggestions = true;
                    this.RaisePropertyChanged(nameof(this.ShowAutoCompleteSuggestions));
                }
            }
        }
        public List<string> NewEntryDescriptionSuggestions
        {
            get => this._newEntryDescriptionSuggestions;
            private set
            {
                this.SetProperty(ref this._newEntryDescriptionSuggestions, value);

                if (this._newEntryDescriptionSuggestions.Count > 0)
                {
                    this.ShowDescriptionAutoCompleteSuggestions = true;
                    this.RaisePropertyChanged(nameof(this.ShowAutoCompleteSuggestions));
                }
            }
        }

        public bool ShowAutoCompleteSuggestions
        {
            get => this.ShowVenueAutoCompleteSuggestions || this.ShowDescriptionAutoCompleteSuggestions;
            set => this.ShowVenueAutoCompleteSuggestions = this.ShowDescriptionAutoCompleteSuggestions = value;
        }
        public bool ShowVenueAutoCompleteSuggestions { get=> this._showVenueAutoCompleteSuggestions; set=>this.SetProperty(ref this._showVenueAutoCompleteSuggestions, value); }
        public bool ShowDescriptionAutoCompleteSuggestions { get => this._showDescriptionAutoCompleteSuggestions; set => this.SetProperty(ref this._showDescriptionAutoCompleteSuggestions, value); }

        public int AutoCompleteSuggestionIndex { get => this._newEntryVenueSuggestionIndex; set => this.SetProperty(ref this._newEntryVenueSuggestionIndex, value); }
        #endregion

        #region Commands    
        public DelegateCommand<int?> AutoCompleteSuggestionIncrementIndexCommand { get; protected set; }

        public DelegateCommand<string> SelectAutoCompleteSuggestionCommand { get; protected set; }

        public DelegateCommand StartNewTransactionBatchCommand { get; private set; }
        public DelegateCommand AddNewEntryToTransactionBatchCommand { get; private set; }
        public DelegateCommand SaveNewTransactionBatchCommand { get; private set; }
        public DelegateCommand ClearNewTransactionBatchCommand { get; private set; }

        public DelegateCommand IncrementNewEntryDateCommand { get; private set; }
        public DelegateCommand DecrementNewEntryDateCommand { get; private set; }
        #endregion

        #region Class Methods
        public NewTransactionBatchViewModel(CalendarViewModel calendarviewmodel) : base()
        {
            try
            {
                this.CalendarViewModel = calendarviewmodel;

                this.SetupCommands();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void SetupCommands()
        {
            this.StartNewTransactionBatchCommand = new DelegateCommand(this.StartNewTransactionBatch);
            this.AddNewEntryToTransactionBatchCommand = new DelegateCommand(this.AddNewEntryToTransactionBatch);
            this.SaveNewTransactionBatchCommand = new DelegateCommand(async () => await this.SaveNewTransactionBatch());
            this.ClearNewTransactionBatchCommand = new DelegateCommand(() => this.NewTransactionBatch?.Clear());

            this.IncrementNewEntryDateCommand = new DelegateCommand(() => this.IncrementNewEntryDate());
            this.DecrementNewEntryDateCommand = new DelegateCommand(() => this.IncrementNewEntryDate(-1));

            this.AutoCompleteSuggestionIncrementIndexCommand = new DelegateCommand<int?>((value) =>
            {
                try
                {
                    if (this.ShowAutoCompleteSuggestions)
                    {
                        if (value == null)
                            value = 1;

                        this.AutoCompleteSuggestionIndex += (int)value;
                    }
                }
                catch { }
            });
            this.SelectAutoCompleteSuggestionCommand = new DelegateCommand<string>((value) =>
            {
                try
                {
                    if (this.ShowVenueAutoCompleteSuggestions)
                    {
                        this.NewEntry.Venue = value;
                        this.RaisePropertyChanged(nameof(NewTransactionBatchViewModel.NewEntryVenue));
                        this.NewEntryDescriptionIsFocused = true;
                    }
                    else if (this.ShowDescriptionAutoCompleteSuggestions)
                    {
                        this.NewEntry.Description = value;
                        this.RaisePropertyChanged(nameof(NewTransactionBatchViewModel.NewEntryDescription));
                        this.NewEntryAccountIsFocused = true;
                    }

                    this.ShowAutoCompleteSuggestions = false;
                }
                catch (Exception ex)
                {
                    MoneyApplication.ErrorHandler(ex);
                }
            });

            //this.SelectDescriptionSuggestionCommand = new DelegateCommand<string>((value) => this.NewEntry.Description = value);

            this.TesterCommand = new DelegateCommand(() => this.NewEntryVenueIsFocused = true);
        }
        #endregion

        #region Data
        public void StartNewTransactionBatch()
        {
            if (this.NewTransactionBatch == null)
                this.NewTransactionBatch = new ObservableCollection<Transaction>();
            this.NewTransactionBatch.Clear();
            this.NewEntry = new Transaction() { TransactionDate = this.CalendarViewModel.SelectedCalenderDate, AccountID = (this.CalendarViewModel.SelectedAccountBalance?.AccountID ?? 0)};
            this.NewEntryDateIsFocused = true;
        }

        public void AddNewEntryToTransactionBatch()
        {
            this.NewEntryDateIsFocused = false;

            if (this.NewEntry.TypeID <= 0)
            {
                this.ShowNewEntryTypeList = true;
            }
            else
            {
                //Create clone of New Entry
                Transaction inserttransaction = new Transaction()
                {
                    TransactionDate = this.NewEntry.TransactionDate,
                    Amount = this.NewEntry.Amount,
                    TypeID = this.NewEntry.TypeID,
                    TransactionType = this.CalendarViewModel.TransactionTypes.FirstOrDefault(type => type.TypeID == this.NewEntry.TypeID),
                    Venue = this.NewEntry.Venue,
                    Description = this.NewEntry.Description,
                    AccountID = this.NewEntry.AccountID,
                    AccountName = this.CalendarViewModel.AccountSelectionList.FirstOrDefault(account => account.AccountID == this.NewEntry.AccountID)?.AccountName,
                    IsCompleted = this.NewEntry.IsCompleted
                };

                this.NewTransactionBatch.Add(inserttransaction);

                //Reset New Entry
                this.NewEntry.Amount = 0;
                this.NewEntry.Venue = "";
                this.NewEntry.Description = "";

                //this.RaisePropertyChanged(nameof(this.TransactionBatchNewEntryAmount));
                this.RaisePropertyChanged(nameof(this.NewEntryVenue));
                this.RaisePropertyChanged(nameof(this.NewEntryDescription));

                this.NewEntryDateIsFocused = true;
            }
        }

        private async Task SaveNewTransactionBatch()
        {
            try
            {
                using (MoneyCalendarEntities context = new MoneyCalendarEntities())
                {
                    foreach (Transaction transaction in this.NewTransactionBatch)
                    {
                        context.Transactions.Attach(transaction);
                        context.Entry(transaction).State = System.Data.Entity.EntityState.Added;
                    }

                    context.SaveChanges();
                }

                this.CalendarViewModel.CreateCalendarTransactionList();

                this.StartNewTransactionBatch();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void IncrementNewEntryDate(int increment = 1)
        {
            try
            {
                this.NewEntry.TransactionDate = this.NewEntry.TransactionDate.AddDays(increment);
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
        #endregion
    }
}
