USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[UpdatePaymentPostedDate]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE PROCEDURE [dbo].[UpdatePaymentPostedDate]
	@PaymentIDIN As varchar(50),
	@PostedDateIN As DateTime,
	@CashReceiptNumIN As Int
AS

UPDATE Payment
Set
	PostedDate = @PostedDateIN,
	CashReceiptNum = CASE WHEN @CashReceiptNumIN > 0 
			THEN @CashReceiptNumIN
			ELSE NULL
		END
WHERE PaymentID = @PaymentIDIN





GO
