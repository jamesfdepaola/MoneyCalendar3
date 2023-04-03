using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCalendar.DataModels
{
	public partial class Account : BindableBasePlus
	{
		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		//public Account()
		//{
		//	Bills = new HashSet<Bill>();
		//	Transactions = new HashSet<Transaction>();
		//}

		#region Source Table Fields
		private int _accountID;
		[Key] public int AccountID { get => this._accountID; set => this.SetProperty(ref this._accountID, value); }
		private string _accountNumber;
		[StringLength(100)] public string AccountNumber { get => this._accountNumber; set => this.SetProperty(ref this._accountNumber, value); }
		private string _routingNumber;
		[StringLength(100)] public string RoutingNumber { get => this._routingNumber; set => this.SetProperty(ref this._routingNumber, value); }
		private string _name;
		[StringLength(100)] public string Name { get => this._name; set => this.SetProperty(ref this._name, value); }
		private string _bank;
		[StringLength(100)] public string Bank { get => this._bank; set => this.SetProperty(ref this._bank, value); }
		private string _description;
		[StringLength(100)] public string Description { get => this._description; set => this.SetProperty(ref this._description, value); }
		private bool _isActive;
		public bool IsActive { get => this._isActive; set => this.SetProperty(ref this._isActive, value); }
		private decimal? _creditLimit;
		[Column(TypeName = "money")] public decimal? CreditLimit { get => this._creditLimit; set => this.SetProperty(ref this._creditLimit, value); }
		private bool _isDefaultOpen;
		public bool IsDefaultOpen { get => this._isDefaultOpen; set => this.SetProperty(ref this._isDefaultOpen, value); }
		private double? _sort;
		public double? Sort { get => this._sort; set => this.SetProperty(ref this._sort, value); }
		private int _accountTypeID;
		public int AccountTypeID { get => this._accountTypeID; set => this.SetProperty(ref this._accountTypeID, value); }
		private decimal? _sharePrice;
		[Column(TypeName = "money")] public decimal? SharePrice { get => this._sharePrice; set => this.SetProperty(ref this._sharePrice, value); }
		private bool _purchaseByCost;
		public bool PurchaseByCost { get => this._purchaseByCost; set => this.SetProperty(ref this._purchaseByCost, value); }
		private decimal? _tradeFee;
		[Column(TypeName = "money")] public decimal? TradeFee { get => this._tradeFee; set => this.SetProperty(ref this._tradeFee, value); }
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Bill> Bills { get; set; }

		public virtual AccountType AccountType { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public virtual ICollection<Transaction> Transactions { get; set; }
	}
}