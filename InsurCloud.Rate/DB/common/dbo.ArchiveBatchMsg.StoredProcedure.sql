USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[ArchiveBatchMsg]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[ArchiveBatchMsg]
	@bUseCommon [bit],
	@sMsgId [nvarchar](18),
	@sProduct [nvarchar](18),
	@sStateCode [nvarchar](18),
	@sArchiveReason [nvarchar](100),
	@sConfirmation [nvarchar](250)
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [ImperialFire_WS].[BatchMsgSP].[ArchiveBatchMsg]
GO
