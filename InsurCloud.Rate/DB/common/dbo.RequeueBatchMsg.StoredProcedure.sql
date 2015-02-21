USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[RequeueBatchMsg]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE Procedure [dbo].[RequeueBatchMsg] (@sBatchMsgID numeric(15))
as

BEGIN

SET IDENTITY_INSERT Common..BatchMsg ON

	INSERT INTO Common..BatchMsg
	(MsgID, MsgEffDate, MsgType, MsgSubType, PolicyID, TermEffDate, PolicyTransactionNum, Product, State, PolicyXML, Param1, Param2, Param3, ParamXML,Confirmation, ProcessedDate, ArchiveReason, UserID, SystemTS)  
	SELECT MsgID, MsgEffDate, MsgType, MsgSubType, PolicyID, TermEffDate, PolicyTransactionNum, Product, State, PolicyXML, Param1, Param2, Param3, ParamXML,null, null, null, UserID, SystemTS 
	FROM Common..BatchMsgArchive
	WHERE MsgID = @sBatchMsgID

	DELETE
	FROM Common..BatchMsgArchive
	WHERE MsgID = @sBatchMsgID
 

SET IDENTITY_INSERT Common..BatchMsg OFF


END
















GO
