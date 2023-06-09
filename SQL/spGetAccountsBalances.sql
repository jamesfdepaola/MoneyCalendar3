USE [MoneyCalendar]
GO
/****** Object:  StoredProcedure [dbo].[spGetAccountsBalances]    Script Date: 2020-04-21 6:21:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[spGetAccountsBalances]
(
	@ShowInactive BIT = 0
)
AS

WITH AccountsWithBalance 
(	
	AccountID, 
	AccountTypeID, 
	AccountName, 
	TypeName, 
	TypeSort,   
	AccountSort,
	IsCashType,
	IsCreditType,
	IsStockType,
	IsIRAType,
	IsGroupSelection, 
	IsDefaultOpen,  
	Balance,
	[Value]
)
AS
(
	SELECT	account.AccountID,
			accounttype.AccountTypeID,
			AccountName = account.Name,
			TypeName = accounttype.Name,
			TypeSort = accounttype.Sort,
			AccountSort = account.Sort,
			accounttype.IsCashType,
			accounttype.IsCreditType,
			accounttype.IsStockType,
			accounttype.IsIRAType,
			IsGroupSelection = CONVERT(BIT, 0),
			account.IsDefaultOpen,
			accounttotals.Balance,
			[Value] = CONVERT(MONEY, accounttotals.Shares * account.SharePrice)

	FROM	Accounts account
		INNER JOIN AccountTypes accounttype ON accounttype.AccountTypeID = account.AccountTypeID
		CROSS APPLY (	SELECT	Balance = SUM(trans.Amount),
								Shares = SUM(trans.Shares)
						FROM	Transactions trans
						WHERE	trans.AccountID = account.AccountID) accounttotals

	WHERE	(
				@ShowInactive = 1 
				OR 
				account.IsActive = 1
			)
		AND	(
				accounttype.IsCashType = 1
				OR
				accounttype.IsCreditType = 1
			)
)

SELECT	*
FROM	AccountsWithBalance

UNION

SELECT	AccountID = NULL,
		accounttype.AccountTypeID,
		AccountName = accounttype.Name,
		TypeName = accounttype.Name,
		TypeSort = accounttype.Sort,
		AccountSort = 0,
		accounttotals.IsCashType,
		accounttotals.IsCreditType,
		accounttotals.IsStockType,
		accounttotals.IsIRAType,
		IsGroupSelection = CONVERT(BIT, 1),
		IsDefaultOpen = CONVERT(BIT, 0),
		accounttotals.Balance,
		accounttotals.[Value]

FROM	AccountTypes accounttype
	CROSS APPLY (	SELECT	Balance = SUM(account.Balance),
							[Value] = SUM(account.[Value]),
							account.IsCashType,
							account.IsCreditType,
							account.IsStockType,
							account.IsIRAType
					FROM	AccountsWithBalance account
					WHERE	account.AccountTypeID = accounttype.AccountTypeID
					GROUP BY
							account.IsCashType,
							account.IsCreditType,
							account.IsStockType,
							account.IsIRAType
					) accounttotals

ORDER BY
		TypeSort,
		IsGroupSelection DESC,
		AccountSort,
		AccountName

