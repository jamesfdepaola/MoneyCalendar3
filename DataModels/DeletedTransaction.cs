using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MoneyCalendar.DataModels
{
	public partial class DeletedTransaction : BindableBasePlus
	{
		#region Source Table Fields
		private int _transactionID;
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)] 
		public int TransactionID { get => this._transactionID; set => this.SetProperty(ref this._transactionID, value); }
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
		public decimal Amount { get => this._amount; set => this.SetProperty<decimal>(ref this._amount, value); }
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
		public decimal? DueAmount { get => this._dueAmount; set => this.SetProperty<decimal?>(ref this._dueAmount, value); }
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
	}
}
