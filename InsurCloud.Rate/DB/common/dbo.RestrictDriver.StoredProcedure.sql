USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[RestrictDriver]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[RestrictDriver]
	@UserID [nvarchar](max),
	@DLN [nvarchar](50),
	@DLNState [nvarchar](2),
	@PolicyID [nvarchar](50),
	@TermEffDate [datetime],
	@ReasonCode [nvarchar](max),
	@ReasonComment [nvarchar](max)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS_DLNService].[DLNServiceSP].[RestrictDriver]
GO
