USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetPolicyBalance]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




create PROCEDURE [dbo].[GetPolicyBalance]
	@PolicyNo	VARCHAR(50)
AS

SELECT ISNULL(Sum(PaymentAmt), 0)
FROM Payment
WHERE CancelledDate IS NULL
  AND PaymentDate >= GetDate()
  AND PolicyNbr = @PolicyNo
  AND PostedDate IS NULL


GO
