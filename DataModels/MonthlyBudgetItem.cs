namespace MoneyCalendar.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MonthlyBudgetItem
    {
        public int MonthlyBudgetItemID { get; set; }

        public int BudgetItemID { get; set; }

        public short BudgetYear { get; set; }

        public short BudgetMonth { get; set; }

        [Column(TypeName = "money")]
        public decimal? Budget { get; set; }

        public virtual BudgetItem BudgetItem { get; set; }
    }
}
