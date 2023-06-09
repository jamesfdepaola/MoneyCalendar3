USE [MoneyCalendar]
GO
/****** Object:  UserDefinedFunction [dbo].[GetCashCreditAccountTotals]    Script Date: 2020-04-21 6:22:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create FUNCTION [dbo].[fnGetCashCreditAccountTotals] 
(
	@DateSelected DATE, 
	@AccountID INT = -1,
	@ShowInactive BIT = 0
)
RETURNS @AccountTotals TABLE
(
	AccountID INT, 
	[Name] VARCHAR(50), 
	IsDefaultOpen BIT, 
	Sort INT, 
	
	CompletedSum MONEY,
	NotCompletedSumPositive MONEY,
	NotCompletedSumNegative MONEY,
	NotCompletedSum MONEY,
	CurrentBalance MONEY,
	
	SelectedDateDueAmountPositive MONEY,
	SelectedDateDueAmountNegative MONEY,
	SelectedDateDueAmount MONEY,
	SelectedDateDueBalance MONEY,
	SelectedDateCompletedSum MONEY, 
	SelectedDateBalance MONEY,
	
	IsCashType BIT,
	IsCreditType BIT,
	IsIRAType BIT,
	IsStockType BIT,
	
	CreditLimit MONEY,
	AvailableCredit MONEY
)
AS
BEGIN

DECLARE @Today date = CONVERT(date, GETDATE())

INSERT INTO @AccountTotals
SELECT	AccountID
		, Name
		, IsDefaultOpen
		, Sort

		, CompletedSum
		, NotCompletedSumPositive
		, NotCompletedSumNegative
		, NotCompletedSum
		, CurrentBalance

		, SelectedDateDueAmountPositive
		, SelectedDateDueAmountNegative
		, SelectedDateDueAmount 
		, SelectedDateDueBalance
		, SelectedDateCompletedSum
		, SelectedDateBalance

		, IsCashType
		, IsCreditType
		, IsIRAType
		, IsStockType

		, CreditLimit = CASE WHEN IsCashType = 1 THEN NULL ELSE CreditLimit END
		, AvailableCredit = CASE WHEN IsCashType = 1 THEN NULL ELSE CreditLimit - SelectedDateDueBalance END
	
FROM	(	SELECT	sub2.*
					, SelectedDateDueBalance = CASE WHEN @DateSelected < @Today
													THEN SelectedDateBalance +SelectedDateDueAmountPositive + SelectedDateDueAmountNegative
													ELSE CurrentBalance +SelectedDateDueAmountPositive + SelectedDateDueAmountNegative
													END
					, SelectedDateDueAmount = SelectedDateDueAmountPositive + SelectedDateDueAmountNegative

			FROM	(	SELECT	sub1.*
								, NotCompletedSum = NotCompletedSumPositive + NotCompletedSumNegative
								, CurrentBalance = CompletedSum + NotCompletedSumPositive + NotCompletedSumNegative

						FROM	(	SELECT	AccountID
											, Name
											, Sort
											, IsDefaultOpen
											, CreditLimit
											, IsCashType
											, IsCreditType
											, IsIRAType
											, IsStockType

											, CompletedSum = SUM(CASE WHEN IsCompleted = 1 THEN Amount ELSE 0 END)
											, NotCompletedSumPositive = SUM(CASE WHEN union1.IsDueType = 0 AND IsCompleted = 0 AND Amount > 0 THEN Amount ELSE 0 END)
											, NotCompletedSumNegative = SUM(CASE WHEN union1.IsDueType = 0 AND IsCompleted = 0 AND Amount < 0 THEN Amount ELSE 0 END)

											, SelectedDateCompletedSum = SUM(CASE	WHEN @DateSelected < @Today
																					THEN CASE	WHEN @DateSelected < @Today AND TransactionDate <= @DateSelected AND union1.IsCompleted = 1 
																								THEN Amount 
																								ELSE 0 END
																					ELSE Amount END)
											, SelectedDateBalance = SUM(CASE	WHEN @DateSelected < @Today AND TransactionDate <= @DateSelected 
																				THEN Amount 
																				ELSE 0 END)

											, SelectedDateDueAmountPositive = SUM(CASE WHEN TransactionDate <= @DateSelected AND DueAmount > 0 AND IsCompleted = 0 AND IsDueType = 1 THEN DueAmount ELSE 0 END)																		
											, SelectedDateDueAmountNegative = SUM(CASE WHEN TransactionDate <= @DateSelected AND DueAmount < 0 AND IsCompleted = 0 AND IsDueType = 1 THEN DueAmount ELSE 0 END)

									FROM	(		SELECT	Transactions.TransactionID,
															Transactions.TransactionDate,
															Transactions.Amount,
															Transactions.DueAmount,
															Transactions.IsCompleted,
			
															Accounts.AccountID, 
															Accounts.Name, 
															Accounts.Sort, 
															Accounts.IsDefaultOpen, 
															Accounts.CreditLimit, 
															AccountTypes.IsCashType, 
															AccountTypes.IsCreditType, 
															AccountTypes.IsIRAType, 
															AccountTypes.IsStockType,
															TransactionTypes.IsDueType

													FROM	Transactions
														INNER JOIN TransactionTypes ON TransactionTypes.TypeID = Transactions.TypeID
														RIGHT JOIN Accounts ON Accounts.AccountID = Transactions.AccountID
														RIGHT JOIN AccountTypes ON AccountTypes.AccountTypeID = Accounts.AccountTypeID
													WHERE	(
																@AccountID = -1
																OR
																Accounts.AccountID = @AccountID
															)
														AND	(
																@ShowInactive = 1 
																OR 
																Accounts.IsActive = 1
															)
														AND	(
																AccountTypes.IsCashType = 1 
																OR 
																AccountTypes.IsCreditType = 1
															)
														--AND	Transactions.TransactionDate <= @DateSelected

													UNION 

													SELECT	Transactions.TransactionID,
															Transactions.TransactionDate,
															Transactions.Amount,
															DueAmount = Transactions.DueAmount * -1,
															Transactions.IsCompleted,
	
															account.AccountID, 
															account.Name, 
															account.Sort, 
															account.IsDefaultOpen, 
															account.CreditLimit, 
															AccountTypes.IsCashType, 
															AccountTypes.IsCreditType, 
															AccountTypes.IsIRAType, 
															AccountTypes.IsStockType,
															TransactionTypes.IsDueType

													FROM	Accounts account
														INNER JOIN AccountTypes ON AccountTypes.AccountTypeID = account.AccountTypeID
														INNER JOIN Bills ON Bills.PayToAccountID = account.AccountID
														INNER JOIN Transactions ON Transactions.BillID = Bills.BillID	
														INNER JOIN TransactionTypes ON TransactionTypes.TypeID = Transactions.TypeID

													WHERE	(
																@AccountID = -1
																OR
																account.AccountID = @AccountID
															)
														AND	(
																@ShowInactive = 1 
																OR 
																account.IsActive = 1
															)
														AND	(
																AccountTypes.IsCashType = 1 
																OR 
																AccountTypes.IsCreditType = 1
															)
														AND	TransactionTypes.IsDueType = 1
														--AND	Transactions.TransactionDate <= @DateSelected
												) union1
									GROUP BY
											union1.AccountID
											, union1.Name
											, union1.Sort
											, union1.IsDefaultOpen
											, union1.CreditLimit
											, union1.IsCashType
											, union1.IsCreditType
											, union1.IsIRAType
											, union1.IsStockType


								) sub1
					) sub2
		) sub3

ORDER BY
		IsCashType DESC,
		IsCreditType DESC,
		IsIRAType DESC,
		IsStockType DESC,
		Sort

RETURN
END
