USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[CloneBatchMessage]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE   PROCEDURE [dbo].[CloneBatchMessage] (@ProductCode as varchar(2), @StateCode as varchar(2), @MsgID as varchar(15))
AS
BEGIN

   --DECLARE @ProductCode varchar(2), @StateCode varchar(2), @MsgID varchar(15)
   --Set @ProductCode = '2'
   --Set @StateCode = '09'
   --Set @MsgID = '4852086'
   
   DECLARE @exeCmd nvarchar(2000), @UserName varchar(30), @ReturnVal bit

   Set @UserName = (Select substring(system_user,patindex('%\%',system_user) + 1,len(system_user) - patindex('%\%',system_user)))
   
   IF @ProductCode = '' or @StateCode = '' or @MsgID = ''
   BEGIN
	  Select '1'
   END
   ELSE
   BEGIN
	  
	  Select @exeCmd = ' INSERT INTO pgm' + @ProductCode + @StateCode + '..batchmsg
	  SELECT MsgEffDate,MsgType,MsgSubType,PolicyID,TermEffDate,PolicyTransactionNum,Product,State,null,Param1,Param2,Param3,ParamXML,null,null,null,'''+@UserName+''',getdate()
	  FROM pgm' + @ProductCode + @StateCode + '..batchmsgarchive WHERE msgid=' + @MsgID

	  EXEC sp_executesql @exeCmd
	  
	  SELECT convert(varchar(10),@@IDENTITY) BatchMsgID
   END


END


/**
 exec [dbo].[CloneBatchMessage] '2','09',4852086

**/




GO
