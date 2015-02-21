USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[AddBatchMsgNoPol]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[AddBatchMsgNoPol]
	@bUseCommon [bit],
	@sMsgType [nvarchar](50),
	@sMsgSubType [nvarchar](50),
	@dtMsgEffDate [datetime],
	@sPolicyID [nvarchar](50),
	@dtTermEffDate [datetime],
	@nTransactionNum [int],
	@bLoadPolicy [bit],
	@sProduct [nvarchar](18),
	@sStateCode [nvarchar](18),
	@sParam1 [nvarchar](250),
	@sParam2 [nvarchar](250),
	@sParam3 [nvarchar](250),
	@sUserId [nvarchar](25)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS].[BatchMsgSP].[AddBatchMsgNoPol]
GO
