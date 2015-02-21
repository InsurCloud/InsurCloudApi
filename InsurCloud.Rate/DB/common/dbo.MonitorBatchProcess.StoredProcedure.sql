USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[MonitorBatchProcess]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- exec Common.dbo.MonitorBatchProcess
-- =============================================
-- Author:		Mark W Depperschmidt
-- Create date: 3-3-2011
-- Description:	Monitor Daily Batch Messages for Failure
-- =============================================
CREATE PROCEDURE [dbo].[MonitorBatchProcess] 
AS
BEGIN
	SET NOCOUNT ON;

    -- Insert statements for procedure here
select ID as BatchID, Process, 
convert(char(10), StartTS, 101) StartTS,
convert(char(10), EndTS, 101) EndTS, 
System, Status, replace(Msg, '''', '"') Msg, 
SystemTS
from common..activitylog
where Status = 'ERROR'
and UserID = 'BATCH'
and SystemTS > getdate() -3
order by ID

END

GO
