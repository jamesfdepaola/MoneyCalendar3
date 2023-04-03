namespace MoneyCalendar.DataModels
{
    public partial class DbSettings : BindableBasePlus
    {
        public int? DefaultAccountID { get; set; }

        public int? BillTypeID { get; set; }

        public int? PaycheckTypeID { get; set; }
    }
}
