using System;
using System.Data.Entity;
using System.Linq;

namespace MoneyCalendar.DataModels
{
    public class MoneyCalendarEntities : DbContext
    {
        public MoneyCalendarEntities()
            : base(MoneyApplication.ConnectionString)
        {
        }
        public MoneyCalendarEntities(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountType> AccountTypes { get; set; }
        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<BudgetItem> BudgetItems { get; set; }
        public virtual DbSet<Earner> Earners { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<MonthlyBudgetItem> MonthlyBudgetItems { get; set; }
        public virtual DbSet<DeletedTransaction> DeletedTransactions { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionType> TransactionTypes { get; set; }
        public virtual DbSet<MonthlyExpenseSet> MonthlyExpenseSets { get; set; }
        public virtual DbSet<MonthlyExpense> MonthlyExpenses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .Property(e => e.CreditLimit)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Account>()
                .Property(e => e.SharePrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Account>()
                .Property(e => e.TradeFee)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.Bills)
                .WithOptional(e => e.Account)
                .HasForeignKey(e => e.DueAccountID);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.Transactions)
                .WithRequired(e => e.Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AccountType>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<AccountType>()
                .HasMany(e => e.Accounts)
                .WithRequired(e => e.AccountType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Bill>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Bill>()
                .Property(e => e.CompanyName)
                .IsUnicode(false);

            modelBuilder.Entity<Bill>()
                .Property(e => e.InstallmentOpenBalance)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Bill>()
                .Property(e => e.DueAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BudgetItem>()
                .Property(e => e.DefaultBudget)
                .HasPrecision(19, 4);

            modelBuilder.Entity<BudgetItem>()
                .HasMany(e => e.MonthlyBudgetItems)
                .WithRequired(e => e.BudgetItem)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Job>()
                .Property(e => e.PayAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<MonthlyBudgetItem>()
                .Property(e => e.Budget)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.SalesTax)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.Principle)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.Interest)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.DueAmount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.SharePrice)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.SharesCost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Transaction>()
                .Property(e => e.SharesOriginalCost)
                .HasPrecision(19, 4);
        }

        public TransactionType GetDefaultTransactionType()
        {
            TransactionType transactiontype = this.TransactionTypes.FirstOrDefault(type => type.IsDefault);

            if (transactiontype == null)
                transactiontype = this.TransactionTypes.First();

            return transactiontype;
        }

        public void SettleDueType(Transaction transaction, decimal? newamount = null, bool isposted = false)
        {
            try
            {
                if (transaction != null)
                {
                    if (newamount != null)
                        transaction.DueAmount = (decimal)newamount;

                    transaction.TypeID = transaction.DueTypeID ?? this.GetDefaultTransactionType().TypeID;
                    transaction.TransactionType = this.TransactionTypes.First(type => type.TypeID == transaction.TypeID);
                    transaction.Amount = transaction.DueAmount ?? 0;

                    if (isposted)
                        transaction.IsCompleted = true;

                    //Check for bill
                    if (transaction.BillID != null)
                    {
                        //Make sure we have the Bill
                        if (transaction.Bill == null)
                            transaction.Bill = this.Bills.First(bill => bill.BillID == transaction.BillID);

                        if (transaction.Bill.PayToAccountID != null)
                        {
                            //Create pay to account transaction
                            Transaction paytoaccounttransaction = new Transaction();
                            paytoaccounttransaction.AccountID = (int)transaction.Bill.PayToAccountID;
                            paytoaccounttransaction.TransactionDate = transaction.TransactionDate;
                            paytoaccounttransaction.TypeID = transaction.Bill.PayToAccountTransactionTypeID ??  this.GetDefaultTransactionType().TypeID;
                            paytoaccounttransaction.Amount = transaction.Amount * -1;
                            paytoaccounttransaction.BillID = transaction.BillID;
                            paytoaccounttransaction.PaidFromAccountCoTransactionID = transaction.TransactionID;
                            paytoaccounttransaction.IsCompleted = transaction.IsCompleted;

                            this.Transactions.Add(paytoaccounttransaction);
                            this.Transactions.Attach(paytoaccounttransaction);
                            this.Entry(paytoaccounttransaction).State = EntityState.Added;

                            this.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
    }
}