USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[AgentMVRPaymentsReport]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[AgentMVRPaymentsReport]
	@AgentID As Varchar(17),
	@StartDate As DateTime,
	@EndDate As DateTime,
	@QuoteID As VarChar(50)
AS
SELECT	Pay.PaymentMethodXML.value('(clsMVRPayment/AgentID)[1]', 'varchar(17)') As agentRecordId,
		Pay.PaymentMethodXML.value('(clsMVRPayment/AgentID)[1]', 'varchar(17)') As AgentID,
		Pay.PolicyNbr,
		Pay.PaymentMethodXML.value('(clsMVRPayment/DriverFirstName)[1]', 'varchar(17)') + ' ' +
			Pay.PaymentMethodXML.value('(clsMVRPayment/DriverLastName)[1]', 'varchar(17)') As "DriverName",
		Pay.PaymentMethodXML.value('(clsMVRPayment/DLN)[1]', 'varchar(17)') As "DLN",
		Pay.PaymentMethodXML.value('(clsMVRPayment/DOB)[1]', 'varchar(17)') As "DOB",
		'$' + Cast(CAST(Pay.PaymentAmt AS MONEY) As Varchar) As "PaymentAmt",
		Pay.PaymentDate As "PaymentDate"
FROM Common..Payment Pay (nolock)
WHERE PaymentMethod = 'AEFTMVR'
	AND PaymentDate BETWEEN @StartDate AND @EndDate
	AND CancelledDate is Null
	AND (@AgentID = '-1' OR Pay.PaymentMethodXML.value('(clsMVRPayment/AgentID)[1]', 'varchar(17)') = @AgentID)
	AND (Datalength(@QuoteID) = 0 OR @QuoteID = Pay.PolicyNbr)





GO
