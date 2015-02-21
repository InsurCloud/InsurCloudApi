USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[NewPrintProcessDailyLog]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- exec Common.dbo.NewPrintProcessDailyLog 
-- =============================================
-- Author:		Mark W Depperschmidt
-- Create date: 11/2/2010
-- Description:	Creates log of New Print Process for balancing with DMP
-- =============================================
CREATE PROCEDURE [dbo].[NewPrintProcessDailyLog]
	
AS
BEGIN
	SET NOCOUNT ON;



declare @ProcessDate char(10)

--set @ProcessDate = convert(char(10), getdate(), 101)

set @ProcessDate = convert(char(10), getdate() -1, 101)

PRINT @ProcessDate

select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
into #tempDMPActivity
from common..activitylog al with (NOLOCK) join
pgm142..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all

select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm117..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm202..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max)) not like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType not in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm202..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max))  like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType  in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm203..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max)) not like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType not in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status



union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm209..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max)) not like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType not in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm209..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max))  like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType  in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status



union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm217..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max)) not like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType not in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm217..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max))  like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType  in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status


union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm235..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and MsgType='Mail' 
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm242..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max)) not like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType not in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status

union all
select ID, StartTs, EndTS, 
convert(datetime, Parm02) as RunDate, Parm03,Parm04 , Status
,count(*) as ItemCount
from common..activitylog al with (NOLOCK) join
pgm242..batchmsgArchive bma with (NOLOCK) 
on Product = Parm03 and State = Parm04 
and bma.ProcessedDate between al.StartTS and dateadd(mi,1,al.EndTS) --and al.StartTS<=(bma.ProcessedDate + .1) and al.EndTS>=bma.ProcessedDate
--and al.StartTS<=bma.ProcessedDate and al.EndTS>=bma.ProcessedDate
where Process='MailProcess' 
and Cast(ArgumentsXML as varchar(max))  like '%RunSR22Mail%'
and MsgType='Mail' 
and MsgsubType  in ('SR22','SR26')
and convert(char(10), convert(datetime, al.Parm02), 101) = @ProcessDate
and ItemCount > 0 and ArchiveReason='Mailed'
group by ID, StartTs, EndTS, convert(datetime, Parm02), Parm03,Parm04 , Status
--create Totals line

insert into #tempDMPActivity
select 999999999, min(StartTs), max(EndTS), 
@ProcessDate, '', '', 'TOTALS', 
sum(ItemCount) from #tempDMPActivity

--select * from #tempDMPActivity

select ID as ZipFileName ,StartTs,EndTS, 
RunDate , Parm03+Parm04 as Program, Status,
ItemCount from #tempDMPActivity
order by ID ,StartTs,EndTS, RunDate, Parm03  ,Parm04  , Status 

drop table #tempDMPActivity

END











GO
