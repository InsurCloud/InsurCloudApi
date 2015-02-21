USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[NewPrintProcessRecon]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- exec Common.dbo.NewPrintProcessRecon
-- =============================================
-- Author:		Mark W Depperschmidt
-- Create date: 11/2/2010
-- Description:	Creates log of New Print Process for balancing with DMP
-- =============================================
CREATE PROCEDURE [dbo].[NewPrintProcessRecon]
	
AS
BEGIN
	SET NOCOUNT ON;



declare @ProcessDate char(10)
declare @PrevMonthStart datetime
declare @PrevMonthEnd datetime

set @PrevMonthStart = (dateadd(month, datediff(month, -1, getdate()) - 2, -1) + 1) 
set @PrevMonthEnd = DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,GETDATE()),0))

PRINT @PrevMonthStart
PRINT @PrevMonthEnd

select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
into #tempDMPActivity
from common..activitylog al with (NOLOCK) join
pgm142..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all

select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm117..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
-- and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm202..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm203..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm209..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm217..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm235..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm242..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
--and al.StartTS <= @PrevMonthEnd and al.EndTS >= @PrevMonthStart
and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
--and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and convert(char(10), convert(datetime, al.Parm02), 101) >= @PrevMonthStart
and convert(char(10), convert(datetime, al.Parm02), 101) < @PrevMonthEnd -1
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

--create Totals line

insert into #tempDMPActivity
select 999999999, min(StartTs), max(EndTS), 
getdate(), '', '', 'TOTALS', 
sum(ItemCount) from #tempDMPActivity

--select * from #tempDMPActivity

--select 
--	case
--		when Parm04 = '02' then 'Arizona     '
--		when Parm04 = '03' then 'Arkansas    '
--		when Parm04 = '09' then 'Florida     '
--		when Parm04 = '17' then 'Louisiana   '
--		when Parm04 = '35' then 'Oklahoma    '
--		when Parm04 = '42' then 'Texas     '
--	else 'Unknown'
--end as StateCode,
--sum(ItemCount), Percent(ItemCount) as ItemCount from #tempDMPActivity
--where ID <> 999999999
--group by parm04
--order by Parm04


SELECT 	
Parm04,
case
		when Parm04 = '02' then 'Arizona  '
		when Parm04 = '03' then 'Arkansas '
		when Parm04 = '09' then 'Florida  '
		when Parm04 = '17' then 'Louisiana'
		when Parm04 = '35' then 'Oklahoma '
		when Parm04 = '42' then 'Texas     '
	else 'Unknown'
end as StateCode,
sum(ItemCount) as ItemCount
from #tempDMPActivity
where ID <> 999999999
group by Parm04




select ID as ZipFileName ,StartTs,EndTS, 
RunDate , Parm03+Parm04 as Program, Status,
ItemCount from #tempDMPActivity
order by ID ,StartTs,EndTS, RunDate, Parm03  ,Parm04  , Status 

--drop table #tempDMPActivity

END












GO
