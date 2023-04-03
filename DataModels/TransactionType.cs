using System.ComponentModel.DataAnnotations;

namespace MoneyCalendar.DataModels
{
    public partial class TransactionType : BindableBasePlus
    {
        [Key]
        public int TypeID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public float? Sort { get; set; }

        public bool IsSystem { get; set; }

        public int? BudgetItemID { get; set; }

        public bool IsNegative { get; set; }

        public bool IsDueType { get; set; }

        public bool IsActive { get; set; }

        public bool IsMoneyType { get; set; }
        public bool IsStockType { get; set; }

        public bool IsStockDividend { get; set; }

        public bool IsDefault { get; set; }
        
        public bool IsBillType { get; set; }
        public bool IsPaycheckType { get; set; }
    }
}
