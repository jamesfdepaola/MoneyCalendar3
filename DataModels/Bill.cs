using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCalendar.DataModels
{
    public partial class Bill : BindableBasePlus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bill()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int BillID { get; set; }

        public int? TypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string CompanyName { get; set; }

        [StringLength(255)]
        public string PayToName { get; set; }

        [StringLength(50)]
        public string AccountNumber { get; set; }

        [StringLength(250)]
        public string Website { get; set; }

        public bool IsActive { get; set; }

        public bool IsConsumerDebt { get; set; }

        public bool IsInstallment { get; set; }

        [Column(TypeName = "money")]
        public decimal? InstallmentOpenBalance { get; set; }

        public bool DueDateLoadingIsEnabled { get; set; }

        public int? DueDay { get; set; }

        [Column(TypeName = "money")]
        public decimal? DueAmount { get; set; }

        public int? DueAccountID { get; set; }

        public bool IsPaidToAccount { get; set; }

        public int? PayToAccountID { get; set; }

        public int? PayToAccountTransactionTypeID { get; set; }

        public virtual Account Account { get; set; }

        public virtual TransactionType TransactionType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
