namespace MoneyCalendar.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class AccountType : BindableBasePlus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AccountType()
        {
            Accounts = new HashSet<Account>();
        }

		#region Source Table Fields
		private int _accountTypeID;
		[Key] public int AccountTypeID { get => this._accountTypeID; set => this.SetProperty(ref this._accountTypeID, value); }
		private string _name;
		[StringLength(50)] public string Name { get => this._name; set => this.SetProperty(ref this._name, value); }
		private bool _isCashType;
		public bool IsCashType { get => this._isCashType; set => this.SetProperty(ref this._isCashType, value); }
		private bool _isCreditType;
		public bool IsCreditType { get => this._isCreditType; set => this.SetProperty(ref this._isCreditType, value); }
		private bool _isStockType;
		public bool IsStockType { get => this._isStockType; set => this.SetProperty(ref this._isStockType, value); }
		private bool _isIRAType;
		public bool IsIRAType { get => this._isIRAType; set => this.SetProperty(ref this._isIRAType, value); }
		private int? _sort;
		public int? Sort { get => this._sort; set => this.SetProperty(ref this._sort, value); }
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
