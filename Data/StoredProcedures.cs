using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MoneyCalendar.DataModels;

namespace MoneyCalendar.Data
{
    public static class StoredProcedures
    {
        public async static Task<List<AccountBalance>> GetAccountsBalances(bool showinactive = false)
        {
            List<AccountBalance> results = null;

            try
            {
                SqlParameter showinactiveparameter = new SqlParameter("@ShowInactive", showinactive);

                using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                {
                    results = await context.Database.SqlQuery<AccountBalance>("spGetAccountsBalances @ShowInactive", showinactiveparameter).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return results;
        }

        public async static Task<List<AccountDatedTotals>> GetAccountTotalsList(DateTime selecteddate , bool showinactive = false)
        {
            List<AccountDatedTotals> results = null;

            try
            {
                SqlParameter dateselectedparameter = new SqlParameter("@DateSelected", showinactive);
                SqlParameter accountidparameter = new SqlParameter("@AccountID", -1);
                SqlParameter showinactiveparameter = new SqlParameter("@ShowInactive", showinactive);

                using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                {
                    results = await context.Database.SqlQuery<AccountDatedTotals>("SELECT * FROM [dbo].[GetCashCreditAccountTotals] (@DateSelected, @AccountID, @ShowInactive) ", dateselectedparameter, accountidparameter, showinactiveparameter).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return results;
        }

        public async static Task<AccountDatedTotals> GetAccountTotals(DateTime selecteddate, int accountid = -1, bool showinactive = false)
        {
            AccountDatedTotals results = null;

            try
            {
                SqlParameter dateselectedparameter = new SqlParameter("@DateSelected", selecteddate);
                SqlParameter accountidparameter = new SqlParameter("@AccountID", accountid);
                SqlParameter showinactiveparameter = new SqlParameter("@ShowInactive", showinactive);

                using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                {
                    results = await context.Database.SqlQuery<AccountDatedTotals>("SELECT * FROM [dbo].[fnGetCashCreditAccountTotals] (@DateSelected, @AccountID, @ShowInactive) ", dateselectedparameter, accountidparameter, showinactiveparameter).FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return results;
        }

        public async static Task<List< MonthlyExpense>> GetMonthlyExpenses(DateTime startdate, DateTime enddate)
        {
            List<MonthlyExpense> results = null;

            try
            {
                SqlParameter startdateparameter = new SqlParameter("@StartDate", startdate);
                SqlParameter enddateparameter = new SqlParameter("@EndDate", enddate);

                using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                {
                    results = await context.Database.SqlQuery<MonthlyExpense>("spMonthlyExpenses @StartDate, @EndDate", startdateparameter, enddateparameter).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return results;
        }

        public async static Task<List<DueTransaction>> GetDueDates(int year, int month)
        {
            List<DueTransaction> results = null;

            try
            {
                SqlParameter yearparameter = new SqlParameter("@Year", year);
                SqlParameter monthparameter = new SqlParameter("@Month", month);

                using (MoneyCalendarEntities context = MoneyApplication.CreateConext())
                {
                    results = await context.Database.SqlQuery<DueTransaction>("spDueDates @Year, @Month", yearparameter, monthparameter).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }

            return results;
        }        
    }

    public class AccountBalance
    {
        public int? AccountID { get; set; }
        public int AccountTypeID { get; set; }
        public string AccountName { get; set; }
        public string TypeName { get; set; }
        public int? TypeSort { get; set; }
        public double? AccountSort { get; set; }
        public bool IsCashType { get; set; }
        public bool IsCreditType { get; set; }
        public bool IsStockType { get; set; }
        public bool IsIRAType { get; set; }
        public bool IsGroupSelection { get; set; }
        //public bool IsDefaultOpen { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Value { get; set; }

        public decimal? DisplayAmount
        {
            get
            {
                if (this.IsCashType || this.IsCreditType)
                    return this.Balance;
                else
                    return this.Value;
            }
        }
    }

    public class AccountDatedTotals
    {
        public int AccountID { get; set; }
        public string Name { get; set; }
        //public bool IsDefaultOpen { get; set; }
        public int Sort { get; set; }
        public decimal CompletedSum { get; set; }
        public decimal NotCompletedSumPositive { get; set; }
        public decimal NotCompletedSumNegative { get; set; }
        public decimal NotCompletedSum { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal SelectedDateDueAmountPositive { get; set; }
        public decimal SelectedDateDueAmountNegative { get; set; }
        public decimal SelectedDateDueAmount { get; set; }
        public decimal SelectedDateDueBalance { get; set; }
        public decimal SelectedDateCompletedSum { get; set; }
        public decimal SelectedDateBalance { get; set; }
        public bool IsCashType { get; set; }
        public bool IsCreditType { get; set; }
        public bool IsIRAType { get; set; }
        public bool IsStockType { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? AvailableCredit { get; set; }
    }

    public class DueTransaction : BindableBasePlus
    {
        public int AccountID { get; set; }
        public int TypeID { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? BillID { get; set; }
        public int? EarnerID { get; set; }
        public int? JobID { get; set; }
        public decimal DueAmount { get; set; }
        public int DueTypeID { get; set; }
        public string BillName { get; set; }
        public string EarnerName { get; set; }
        public string Employer { get; set; }
        public bool Include { get; set; }
    }
}
