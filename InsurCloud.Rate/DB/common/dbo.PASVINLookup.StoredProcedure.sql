USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[PASVINLookup]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[PASVINLookup]
	@sVIN [nvarchar](17),
	@dtRateEffDate [datetime]
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS_VINService].[VINServiceSP].[PASVINLookup]
GO
