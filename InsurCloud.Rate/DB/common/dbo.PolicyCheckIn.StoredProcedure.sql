USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[PolicyCheckIn]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[PolicyCheckIn]
	@sPolicyWizardNum [nvarchar](50),
	@sPolicyID [nvarchar](17),
	@dTermEffDate [datetime],
	@sUserType [nvarchar](50),
	@sUserID [nvarchar](50)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS_PolLibraryService].[PolLibraryServiceSP].[PolicyCheckIn]
GO
