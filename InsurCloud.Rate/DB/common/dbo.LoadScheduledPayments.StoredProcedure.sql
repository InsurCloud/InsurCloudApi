USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[LoadScheduledPayments]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO









CREATE Procedure [dbo].[LoadScheduledPayments]
	@PolicyList		VARCHAR(7000)
AS

DECLARE @PolicyTable TABLE (ID VARCHAR(50))
INSERT INTO @PolicyTable SELECT * FROM dbo.fn_Split_Up_Ids(@PolicyList)

SELECT PaymentID, PolicyNbr, CASE PaymentMethod
								WHEN 'CC' THEN 'Credit Card'
								ELSE PaymentMethod END AS PaymentMethod, PaymentAmt, PaymentDate
FROM Payment cp
INNER JOIN @PolicyTable tbl ON tbl.ID = cp.PolicyNbr
WHERE CancelledDate IS NULL
 -- AND PaymentDate > GetDate()
	AND PostedDate is null
	and paymentmethod <> 'aeftmvr'
ORDER BY PaymentDate

SELECT PaymentID, PolicyNbr, CASE PaymentMethod
								WHEN 'CC' THEN 'Credit Card'
								ELSE PaymentMethod END AS PaymentMethod, PaymentAmt, PostedDate
FROM Payment cp
INNER JOIN @PolicyTable tbl ON tbl.ID = cp.PolicyNbr
WHERE CancelledDate IS NULL
  AND PostedDate IS NOT NULL
  AND PostedDate < GetDate()
  and paymentmethod <> 'aeftmvr'
ORDER BY PostedDate








GO
