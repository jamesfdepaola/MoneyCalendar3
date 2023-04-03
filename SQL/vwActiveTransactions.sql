USE [MoneyCalendar]
GO

/****** Object:  View [dbo].[ActiveTransactions]    Script Date: 2020-04-21 6:19:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

drop VIEW [dbo].[ActiveTransactions]
	AS 
	SELECT	Transactions.*
			, AccountIsActive = Accounts.IsActive
			, TransactionTypes.IsDebit
			, TransactionTypes.IsDueType
			, TransactionTypes.IsStockType
			, TransactionTypes.IsStockDividend
			, TransactionTypes.[Name]

			, TotalCostLessDividends = CASE WHEN TransactionTypes.IsStockDividend = 1 OR TransactionTypes.IsDebit = 1 THEN 0 ELSE Transactions.SharesCost END
			, TotalDividends = CASE WHEN TransactionTypes.IsStockDividend = 1 THEN Transactions.SharesCost ELSE 0 END

	FROM	Transactions
		INNER JOIN Accounts ON Accounts.AccountID = Transactions.AccountID
		INNER JOIN TransactionTypes ON TransactionTypes.TypeID = Transactions.TypeID 
 	WHERE	Transactions.IsDeleted = 0


GO


