using MoneyCalendar.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace MoneyCalendar.DataModels
{
	public partial class Transaction : BindableBasePlus
	{
		#region Constructors
		public Transaction()
		{
			this.TransactionDate = DateTime.Today;
		}
		#endregion

		#region Source Table Fields
		private int _transactionID;
		[Key] public int TransactionID { get => this._transactionID; set => this.SetProperty(ref this._transactionID, value); }
		private int _accountID;
		public int AccountID { get => this._accountID; set => this.SetProperty(ref this._accountID, value); }
		private int _typeID;
		public int TypeID { get => this._typeID; set => this.SetProperty(ref this._typeID, value); }
		private DateTime _transactionDate;
		public DateTime TransactionDate { get => this._transactionDate; set => this.SetProperty(ref this._transactionDate, value); }
		private string _description;
		[StringLength(2000)] public string Description { get => this._description; set => this.SetProperty(ref this._description, value); }
		private decimal _amount;
		[Column(TypeName = "money")]
		public decimal Amount
		{
			get => this._amount;
			set => this.SetProperty<decimal>(ref this._amount, this.FixAmountSign(value) ?? 0);
		}
		private decimal? _salesTax;
		[Column(TypeName = "money")] public decimal? SalesTax { get => this._salesTax; set => this.SetProperty(ref this._salesTax, value); }
		private decimal? _principle;
		[Column(TypeName = "money")] public decimal? Principle { get => this._principle; set => this.SetProperty(ref this._principle, value); }
		private decimal? _interest;
		[Column(TypeName = "money")] public decimal? Interest { get => this._interest; set => this.SetProperty(ref this._interest, value); }
		private string _venue;
		[StringLength(100)] public string Venue { get => this._venue; set => this.SetProperty(ref this._venue, value); }
		private int? _billID;
		public int? BillID { get => this._billID; set => this.SetProperty(ref this._billID, value); }
		private int? _earnerID;
		public int? EarnerID { get => this._earnerID; set => this.SetProperty(ref this._earnerID, value); }
		private int? _jobID;
		public int? JobID { get => this._jobID; set => this.SetProperty(ref this._jobID, value); }
		private int? _checkNumber;
		public int? CheckNumber { get => this._checkNumber; set => this.SetProperty(ref this._checkNumber, value); }
		private bool _isCompleted;
		public bool IsCompleted { get => this._isCompleted; set => this.SetProperty(ref this._isCompleted, value); }
		private DateTime? _checkClearDate;
		public DateTime? CheckClearDate { get => this._checkClearDate; set => this.SetProperty(ref this._checkClearDate, value); }
		private decimal? _dueAmount;
		[Column(TypeName = "money")]
		public decimal? DueAmount
		{
			get => this._dueAmount;
			set => this.SetProperty<decimal?>(ref this._dueAmount, this.FixAmountSign(value));
		}
		private int? _dueTypeID;
		public int? DueTypeID { get => this._dueTypeID; set => this.SetProperty(ref this._dueTypeID, value); }
		private int? _transferCoTransactionID;
		public int? TransferCoTransactionID { get => this._transferCoTransactionID; set => this.SetProperty(ref this._transferCoTransactionID, value); }
		private int? _paidFromAccountCoTransactionID;
		public int? PaidFromAccountCoTransactionID { get => this._paidFromAccountCoTransactionID; set => this.SetProperty(ref this._paidFromAccountCoTransactionID, value); }
		private double? _shares;
		public double? Shares { get => this._shares; set => this.SetProperty(ref this._shares, value); }
		private decimal? _sharePrice;
		[Column(TypeName = "money")] public decimal? SharePrice { get => this._sharePrice; set => this.SetProperty(ref this._sharePrice, value); }
		private decimal? _sharesCost;
		[Column(TypeName = "money")] public decimal? SharesCost { get => this._sharesCost; set => this.SetProperty(ref this._sharesCost, value); }
		private int? _paidFromAccountID;
		public int? PaidFromAccountID { get => this._paidFromAccountID; set => this.SetProperty(ref this._paidFromAccountID, value); }
		private decimal? _sharesOriginalCost;
		[Column(TypeName = "money")] public decimal? SharesOriginalCost { get => this._sharesOriginalCost; set => this.SetProperty(ref this._sharesOriginalCost, value); }
		private bool _isTransferFrom;
		public bool IsTransferFrom { get => this._isTransferFrom; set => this.SetProperty(ref this._isTransferFrom, value); }

		#endregion

		#region Members
		private TransactionType _transactionType;
		#endregion

		#region Calculated Properties
		[ObservesProperty(nameof(Transaction.Description))]
		[ObservesProperty(nameof(Transaction.Amount))]
		[ObservesProperty(nameof(Transaction.DueAmount))]
		public string DisplayText
		{
			get
			{
				string displaytext = "";

				if (this.TransactionType?.IsDueType ?? false)
					displaytext = $"{this.AbsoluteDueAmount.ToString("c")} - ";
				else
					displaytext = $"{this.AbsoluteAmount.ToString("c")} - ";

				if (string.IsNullOrEmpty(this.Description))
				{
					if (this.BillID != null)
						displaytext += this.Bill?.Name;

					if (this.EarnerID != null)
						displaytext += this.Earner?.Name;
					if (this.JobID != null)
						displaytext += " " + this.Job?.Employer;
				}
				else
					displaytext += this.Description;

				return displaytext;
			}
		}

		[ObservesProperty(nameof(Transaction.Description))]
		[ObservesProperty(nameof(Transaction.Venue))]
		[ObservesProperty(nameof(Transaction.Bill))]
		public string FullDescription
		{
			get
			{
				string fulldescription = this.Venue;

				if (this.Bill != null)
				{
					fulldescription += "\n" + this.Bill.Name;
				}
				else
					fulldescription += "\n" + this.Description;

				return fulldescription;
			}
		}
		#endregion

		#region Navigation Properties
		[ForeignKey(nameof(Transaction.AccountID))]
		public Account Account { get; set; }
		
		[NotMapped]
		public string AccountName { get; set; }

		[ForeignKey(nameof(Transaction.TypeID))]
		public TransactionType TransactionType
		{
			get => this._transactionType;
			set
			{
				if (value != null)
				{
					//Check if changing the type requires fixing amount sign (we don't need to fix is this is the first time the navigation object is set)
					bool fixamountsign = this.TransactionType != null && value.IsNegative != this.TransactionType.IsNegative;

					this.SetProperty(ref this._transactionType, value);

					if (fixamountsign && (this.TransactionType?.IsDueType ?? false))
						this.DueAmount = this.DueAmount;
					else
						this.Amount = this.Amount;
				}
			}
		}

		[NotMapped]
		public string TypeName { get; set; }

		[ForeignKey(nameof(Transaction.TransferCoTransactionID))]
		public Transaction TransferCoTransaction { get; set; }

		[ForeignKey(nameof(Transaction.DueTypeID))]
		public TransactionType DueType { get; set; }

		[ForeignKey(nameof(Transaction.BillID))]
		public Bill Bill { get; set; }

		[NotMapped]
		public string BillName { get; set; }

		[ForeignKey(nameof(Transaction.JobID))]
		public Job Job { get; set; }

		[ForeignKey(nameof(Transaction.EarnerID))]
		public Earner Earner { get; set; }
		#endregion

		#region UI Properties
		public decimal AbsoluteAmount { get => Math.Abs(this.Amount); }

		public decimal AbsoluteDueAmount { get => Math.Abs(this.DueAmount ?? 0); }

		[ObservesProperty(nameof(Transaction.Amount))]
		public SolidColorBrush StatusColor
		{
			get
			{
				SolidColorBrush statuscolor = Brushes.Transparent;

				if (this.Amount > 0)
					statuscolor = Brushes.Lime;

				else if (!(this.TransactionType?.IsDueType) ?? false)
				{
					if (this.TransactionDate < DateTime.Now && !this.IsCompleted)
						statuscolor = Brushes.Aqua;
				}

				else if (this.TransactionType?.IsDueType ?? false)
				{
					if (this.TransactionType.IsPaycheckType)
						statuscolor = Brushes.PaleGreen;

					else if (this.TransactionDate < DateTime.Now)
						statuscolor = Brushes.Red;
				
					else if (this.TransactionDate.Subtract(DateTime.Now).Days < 7)
						statuscolor = Brushes.Yellow;
					
					else
						statuscolor = Brushes.Orange;
				}

				return statuscolor;
			}
		}
		#endregion

		#region Methods
		private decimal? FixAmountSign(decimal? newvalue)
		{
			if (newvalue == null)
				return null;

			decimal value = (decimal)newvalue;

			//Don't do anything if there's no TransactionType navigation object
			if (this.TransactionType != null)
			{
				//determine if this Transaction is debit or credit
				if (this.TransactionType.IsNegative  || this.IsTransferFrom)
					value = Math.Abs(value) * -1;
				else
					value = Math.Abs(value);
			}

			return value;		
		}
		#endregion
	}
}
