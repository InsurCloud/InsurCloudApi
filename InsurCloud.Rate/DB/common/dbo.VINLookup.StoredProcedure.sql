USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[VINLookup]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[VINLookup]
	@sVIN [nvarchar](17)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS_VINService].[VINServiceSP].[VINLookup]
GO
