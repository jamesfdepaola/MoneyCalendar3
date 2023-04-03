using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyCalendar.DataModels
{
    public partial class MonthlyExpenseSet : BindableBasePlus
    {
		#region Source Table Fields
		private int _monthlyExpenseSetID;
		[Key] public int MonthlyExpenseSetID { get => this._monthlyExpenseSetID; set => this.SetProperty(ref this._monthlyExpenseSetID, value); }
		private DateTime _startDate;
		public DateTime StartDate { get => this._startDate; set => this.SetProperty(ref this._startDate, value); }
		private DateTime _endDate;
		public DateTime EndDate { get => this._endDate; set => this.SetProperty(ref this._endDate, value); }
        #endregion

        #region Properties
		public string DisplayText { get => $"{this.StartDate.ToString("yyyy/M/d")} - {this.EndDate.ToString("yyyy/M/d")}"; }
		#endregion
	}
}
