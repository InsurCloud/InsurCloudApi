USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetCreditCardPaymentsToProcess]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[GetCreditCardPaymentsToProcess]
	@AsOfDateIN As DateTime
AS

SELECT
	PaymentID,
	PolicyNbr,
	PaymentMethod,
	PaymentAmt,
	PaymentDate,
	PaymentMethodXML
FROM Payment
WHERE Convert(datetime,Convert(varchar(10),PostedDate,120)) =  Convert(datetime,@AsOfDateIN)
AND PaymentMethod = 'CC'



GO
