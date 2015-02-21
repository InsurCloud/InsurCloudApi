USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetNACHAMVRData]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[GetNACHAMVRData]
	@AsOfDateIN As DateTime
AS

IF @AsOfDateIN <= '05/07/2010'
	BEGIN
		SELECT	Pay.PaymentMethodXML.value('(//AgentID)[1]', 'varchar(20)')  As AgentCode,
				bank.AgencyName As InsuredName,
				Pay.PolicyNbr,
				Pay.PaymentAmt,
				Pay.PaymentMethodXML,
				bank.AccountNumber,
				bank.RoutingNumber,
				Pay.PaymentID
		FROM Common..Payment Pay
					Inner JOIN MarketingCRM..vwAgencyEFTBankInfo bank on bank.AgencyCode = pay.PaymentMethodXML.value('(//AgentID)[1]', 'varchar(20)')
		WHERE PaymentMethod = 'AEFTMVR'
			AND (Convert(datetime,Convert(varchar(10),SweepDate,120)) =  Convert(datetime,@AsOfDateIN) or Convert(datetime,Convert(varchar(10),ReprocessedDate,120)) =  Convert(datetime,@AsOfDateIN))
			AND CancelledDate is Null
			AND Pay.PolicyNbr <> ''
	END
ELSE
	BEGIN

		SELECT	Pay.PaymentMethodXML.value('(//AgentID)[1]', 'varchar(20)')  As AgentCode,
				bank.AgencyName As InsuredName,
				Pay.PolicyNbr,
				Pay.PaymentAmt,
				Pay.PaymentMethodXML,
				bank.AccountNumber,
				bank.RoutingNumber,
				Pay.PaymentID
		FROM Common..Payment Pay
					Inner JOIN MarketingCRM..vwAgencyEFTBankInfo bank on bank.AgencyCode = pay.PaymentMethodXML.value('(//AgentID)[1]', 'varchar(20)')
		WHERE PaymentMethod = 'AEFTMVR'
			AND (Convert(datetime,Convert(varchar(10),SweepDate,120)) =  Convert(datetime,@AsOfDateIN) or Convert(datetime,Convert(varchar(10),ReprocessedDate,120)) =  Convert(datetime,@AsOfDateIN))
			AND CancelledDate is Null
	
	END

GO
