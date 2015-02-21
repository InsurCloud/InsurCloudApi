USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[PolicyCheckOut]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[PolicyCheckOut]
	@sPolicyID [nvarchar](17),
	@dTermEffDate [datetime],
	@dTransEffDate [datetime],
	@sUserType [nvarchar](50),
	@sUserID [nvarchar](50),
	@sReturnMsg [nvarchar](250) OUTPUT
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS_PolLibraryService].[PolLibraryServiceSP].[PolicyCheckOut]
GO
