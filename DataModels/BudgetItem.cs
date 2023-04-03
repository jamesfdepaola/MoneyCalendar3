namespace MoneyCalendar.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BudgetItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BudgetItem()
        {
            MonthlyBudgetItems = new HashSet<MonthlyBudgetItem>();
        }

        public int BudgetItemID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public bool IsIncome { get; set; }

        [Column(TypeName = "money")]
        public decimal? DefaultBudget { get; set; }

        public float? Sort { get; set; }

        public bool IsActive { get; set; }

        public bool IsMinProjected { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MonthlyBudgetItem> MonthlyBudgetItems { get; set; }
    }
}
