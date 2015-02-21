USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetPaymentsToProcess]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
















CREATE PROCEDURE [dbo].[GetPaymentsToProcess]
	@AsOfDateIN As DateTime
AS

SELECT
	PaymentID,
	PolicyNbr,
	PaymentMethod,
	PaymentAmt,
	PaymentDate,
	SweepDate,
	IsNull(Comments,'') as Comments,
	PaymentMethodXML,
	UserID,
	ReprocessFlag,
	SourceSystem,
	Type as SubType
FROM Payment with (nolock)
WHERE CancelledDate is Null
AND ((PostedDate is Null AND PaymentDate <= @AsOfDateIN + '23:59') OR ReprocessFlag = 1)
And PaymentMethod <> 'NSF'
AND SweepDate is not null
and PolicyNbr not like 'P517%'
and (IsSuspended is null or IsSuspended=0)







GO
