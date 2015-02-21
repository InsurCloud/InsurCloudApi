USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[UnRestrictDriver]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[UnRestrictDriver]
	@UserID [nvarchar](max),
	@DLN [nvarchar](50),
	@DLNState [nvarchar](2),
	@PolicyID [nvarchar](50),
	@TermEffDate [datetime]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS_DLNService].[DLNServiceSP].[UnRestrictDriver]
GO
