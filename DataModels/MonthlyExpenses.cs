using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCalendar.DataModels
{
	public partial class MonthlyExpense : BindableBasePlus
	{
		#region Source Table Fields
		private int _monthlyExpenseID;
		[Key] public int MonthlyExpenseID { get => this._monthlyExpenseID; set => this.SetProperty(ref this._monthlyExpenseID, value); }

		private int _monthlyExpenseSetID;
		public int MonthlyExpenseSetID { get => this._monthlyExpenseSetID; set => this.SetProperty(ref this._monthlyExpenseSetID, value); }
		private string _description;
		[StringLength(200)] public string Description { get => this._description; set => this.SetProperty(ref this._description, value); }
		private string _typeName;
		[StringLength(50)] public string TypeName { get => this._typeName; set => this.SetProperty(ref this._typeName, value); }
		private bool _isIncluded;
		public bool IsIncluded { get => this._isIncluded; set => this.SetProperty(ref this._isIncluded, value); }
		private decimal _amount;
		[Column(TypeName = "money")] public decimal Amount { get => this._amount; set => this.SetProperty(ref this._amount, value); }
		private decimal _splitRate;
		public decimal SplitRate
		{
			get => this._splitRate;
			set
			{
				this.SetProperty(ref this._splitRate, value);
				this.CalculateAmounts();
			}
		}
		private bool _isJointExpense;
		public bool IsJointExpense
		{
			get => this._isJointExpense;
			set
			{
				this.SetProperty(ref this._isJointExpense, value);
				this.CalculateAmounts();
			}
		}
		private decimal _joinerAmount;
		[Column(TypeName = "money")]
		public decimal JoinerAmount
		{
			get => this._joinerAmount;
			set
			{
				this.SetProperty(ref this._joinerAmount, value);
				this.CalculateAmounts();
			}
		}
		private decimal _contribution;
		[Column(TypeName = "money")]
		public decimal Contribution
		{
			get => this._contribution;
			set
			{
				this.SetProperty(ref this._contribution, value);
				this.CalculateAmounts();
			}
		}
		private decimal _splitAmount;
		[Column(TypeName = "money")] public decimal SplitAmount { get => this._splitAmount; set => this.SetProperty(ref this._splitAmount, value); }
		private decimal _owedAmount;
		[Column(TypeName = "money")] public decimal OwedAmount { get => this._owedAmount; set => this.SetProperty(ref this._owedAmount, value); }
		#endregion

		#region Methods
		public void CalculateAmounts()
		{
			this.SplitAmount = (this.Amount + this.JoinerAmount) * this.SplitRate;
			this.OwedAmount = this.SplitAmount - this.JoinerAmount - this.Contribution;
		}
		#endregion
	}
}

//	public class MonthlyExpense : INotifyPropertyChanged
//	{
//		private decimal _splitRate;
//		private bool _isJointExpense;
//		private decimal _joinerAmount;
//		private decimal _contribution;

//		public decimal Amount { get; set; }
//		public string TypeName { get; set; }
//		public string BillName { get; set; }
//		public string Description { get; set; }
//		public string DisplayText { get; set; }
//		public bool IsIncluded { get; set; }
//		public bool IsJointExpense
//		{
//			get => this._isJointExpense;
//			set
//			{
//				this._isJointExpense = value;

//				if (this.PropertyChanged != null)
//					this.RaisePropertiesChanged();
//			}
//		}
//		public decimal JoinerAmount
//		{
//			get => (this.IsJointExpense ? this._joinerAmount : 0);
//			set
//			{
//				this._joinerAmount = value;

//				if (this.PropertyChanged != null)
//					this.RaisePropertiesChanged();
//			}
//		}
//		public decimal Contribution
//		{
//			get => this._contribution;
//			set
//			{
//				this._contribution = value;

//				if (this.PropertyChanged != null)
//					this.RaisePropertiesChanged();
//			}
//		}
//		public decimal SplitRate
//		{
//			get => this._splitRate;
//			set
//			{
//				this._splitRate = value;

//				if (this.PropertyChanged != null)
//					this.RaisePropertiesChanged();
//			}
//		}

//		public decimal SplitAmount { get => (this.Amount + this.JoinerAmount) * this.SplitRate; }
//		public decimal OwedAmount { get => this.SplitAmount - this.JoinerAmount - this.Contribution; }

//		public event PropertyChangedEventHandler PropertyChanged;

//		private void RaisePropertiesChanged()
//		{
//			this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(MonthlyExpense.SplitRate)));
//			this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(MonthlyExpense.SplitAmount)));

//			this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(MonthlyExpense.IsJointExpense)));
//			this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(MonthlyExpense.JoinerAmount)));

//			this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(MonthlyExpense.Contribution)));

//			this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(MonthlyExpense.OwedAmount)));
//		}
//	}
//}