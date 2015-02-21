USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetNSFsToProcess]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE PROCEDURE [dbo].[GetNSFsToProcess]
	@AsOfDateIN As DateTime
AS

SELECT
	PaymentID,
	PolicyNbr,
	PaymentMethod,
	PaymentAmt,
	PaymentDate,
	IsNull(Comments,'') as Comments,
	CashReceiptNum,
	PaymentMethodXML,
	UserID
FROM Payment with (nolock)
WHERE CancelledDate is Null
AND PostedDate is Null
AND PaymentDate <= @AsOfDateIN + '23:59'
And PaymentMethod = 'NSF'










GO
