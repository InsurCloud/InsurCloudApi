USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetBatchMsgsByType]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[GetBatchMsgsByType]
	@bUseCommon [bit],
	@sMsgType [nvarchar](50),
	@sMsgSubType [nvarchar](50),
	@dtMsgEffDate [datetime],
	@sProduct [nvarchar](18),
	@sStateCode [nvarchar](18)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS].[BatchMsgSP].[GetBatchMsgsByType]
GO
