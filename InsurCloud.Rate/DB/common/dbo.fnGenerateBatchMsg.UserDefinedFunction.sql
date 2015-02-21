USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGenerateBatchMsg]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- ===============================================================================
-- Author:		Chetan Swaroop
-- Created date: 7/16/2009
-- Description:	Given a range of dates, it generates records for BatchMsg table.
--              Indicate the msgtype and the function will generate records for
--              msg subtype Policy. You can use the resulting rows to insert
--              records into BatchMsg table. It is very helpful while loading
--              historical data for processes that are driven by run dates
--              through BatchMsg table.
--
-- Usage: Insert BatchMsg
--        Select * from [dbo].[fnGenerateBatchMsg] ('2001-01-01','2003-12-31','DUTP')
-- ==================================================================================


CREATE FUNCTION [dbo].[fnGenerateBatchMsg]
(
	@StartDt smalldatetime = null,
    @EndDt smalldatetime = null,
	@MsgType varchar(50)
	
)
RETURNS @rtnBatchMsgRecords TABLE 
(
	MsgEffDate datetime,
	MsgType varchar(50),
	MsgSubType varchar(50),
	PolicyID nvarchar(50),
	TermEffDate datetime,
	PolicyTransactionNum numeric(2, 0),
	Product numeric(18, 0),
	State numeric(18, 0),
	PolicyXML xml NULL,
	Param1 nvarchar(50),
	Param2 nvarchar(50),
	Param3 nvarchar(50),
	ParamXML xml NULL,
	Confirmation nvarchar(250),
	ProcessedDate datetime,
	ArchiveReason varchar(100),
	UserID varchar(25),
	SystemTS datetime
)
AS
BEGIN
	DECLARE @RunDt smalldatetime
	DECLARE	@UserID VARCHAR(25)

	SET @RunDt = @StartDt;

	WHILE (@RunDt <= @EndDt)
		BEGIN
			IF (SELECT 1 FROM BatchMsg WHERE MsgEffDate=@RunDt and MsgType=@MsgType)=1
				BEGIN
					SELECT @RunDt = dateadd("d", 1, @RunDt)
					CONTINUE
				END
			ELSE
				BEGIN
					insert @rtnBatchMsgRecords (MsgEffDate,	MsgType, MsgSubType,PolicyID,TermEffDate,
							PolicyTransactionNum,Product,State,PolicyXML,Param1,Param2,Param3,ParamXML,
							Confirmation,ProcessedDate,ArchiveReason,UserID,SystemTS)
					values(@RunDt, @MsgType, 'Policy', null, '1900-01-01', 0,
					0, 0, null,null,null,null,null,null,'1900-01-01',null, SUSER_SNAME(), getdate())
				END
			SELECT @RunDt = dateadd("d", 1, @RunDt)
		END
	RETURN 
END






GO
