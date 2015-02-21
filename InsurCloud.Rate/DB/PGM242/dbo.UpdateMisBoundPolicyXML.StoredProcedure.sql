USE [pgm242]
GO
/****** Object:  StoredProcedure [dbo].[UpdateMisBoundPolicyXML]    Script Date: 7/26/2014 4:28:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Shaun Herschbach
-- Create date: 2/18/2014
-- Description:	Used to update the Status column and Status property of PolicyXML column 
--				of PolicyXML rows that were bound but not properly updated, and thus mark
--				them as bound
-- =============================================
CREATE PROCEDURE [dbo].[UpdateMisBoundPolicyXML]
	-- Add the parameters for the stored procedure here

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Mark as Bound
	update policyxml
	set Status='Bound',policyxml.modify('replace value of (/clsPolicyPPA/Status/text())[1] with ("Bound") ')
	where UploadTS is not null and status<>'Bound' and policyid is not null
	and Status='4'
	
	select @@ROWCOUNT UpdateCount
END


GO
