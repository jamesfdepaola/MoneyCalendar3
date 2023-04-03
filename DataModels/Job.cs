using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCalendar.DataModels
{
    public partial class Job : BindableBasePlus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Job()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int JobID { get; set; }

        public int? EarnerID { get; set; }

        [StringLength(50)]
        public string Employer { get; set; }

        public bool IsActive { get; set; }

        public bool PayDateLoadingIsEnabled { get; set; }

        [StringLength(50)]
        public string PayPeriod { get; set; }

        public int? PayDay { get; set; }

        public int? PayDay2 { get; set; }

        [Column(TypeName = "money")]
        public decimal? PayAmount { get; set; }

        public int? PayAccountID { get; set; }

        public virtual Earner Earner { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
