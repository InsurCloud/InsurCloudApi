USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[NPPCertifiedMailReceipt]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- exec [dbo].[NPPCertifiedMailReceipt]

-- =============================================
-- Author:		Mark W Depperschmidt
-- Create date: 10/18/2010
-- Description:	Replaces CertReg.txt for New Print Process
-- =============================================
CREATE PROCEDURE [dbo].[NPPCertifiedMailReceipt] 
AS
BEGIN
	SET NOCOUNT ON;

create table #tempCRTR
(PolicyNo varchar(25),
 Name	  varchar(100),
 Address  varchar(50),
 City     varchar(30),
 State	  varchar(5),
 Zip	  varchar(10),
 MailDate varchar(20),
 CertifiedNo varchar(25),
 Sort     varchar(10)
)

--insert into #tempCRTR
--select 'Imperial Fire & Casualty', ' ', 'Certified Mail Receipt Listing',
--' ', ' ', 'Printed: ', convert(char(10), getdate(), 101), '', '0'

--insert into #tempCRTR
--select 'PolicyNo', 'Name', 'Address',
--'City', 'State', 'Zip Code', 'Mail Date', 'Certified Mail Receipt No', '0'

--select * from #tempCRTR


insert into #tempCRTR
select distinct PolicyNo, EntityName, StatementAddress1, StatementCity, StatementStateCode, StatementZip,
convert(char(10), msg.SystemTS, 101) MailDate, '______________________' as CertifiedNo, Param3
--
from pgm217.dbo.BatchMsgArchive msg (nolock)
--
inner join common..activitylog al (nolock)
on MsgType = 'Mail'
and al.StartTS<=msg.ProcessedDate and al.EndTS>=msg.ProcessedDate
inner join PasCarrier.dbo.Policy p (nolock)
on msg.PolicyID = p.PolicyNo collate database_default 
and msg.TermEffDate = p.TermEffDate
and msg.PolicyTransactionNum = p.PolicyTransactionNum
inner join PasCarrier.dbo.Entity e (nolock)
on p.InsuredEntityNum = e.EntityNum
where MsgType = 'Mail'
and ArchiveReason = 'Mailed'
and MsgSubType = 'Cancellation'
and convert(char(10), Parm02, 101) = convert(char(10), getdate() , 101)
and Confirmation like '%CRTR%'
and left(Param3, 1) = 'I'



union all
select distinct p.PolicyNo, EntityName, StatementAddress1, StatementCity, StatementStateCode, StatementZip,
convert(char(10), SystemTS, 101) MailDate, '______________________' as CertifiedNo, Param3
from pgm217.dbo.BatchMsgArchive msg (nolock)
inner join PasCarrier.dbo.Policy p (nolock)
on msg.PolicyID = p.PolicyNo collate database_default 
and msg.TermEffDate = p.TermEffDate
and msg.PolicyTransactionNum = p.PolicyTransactionNum
inner join PasCarrier.dbo.PolicyEntity pe (nolock)
on msg.PolicyID = pe.PolicyNo collate database_default 
and msg.TermEffDate = pe.TermEffDate
and msg.PolicyTransactionNum = pe.PolicyTransactionNum
inner join PasCarrier.dbo.Entity e (nolock)
on pe.EntityNum = e.EntityNum
where MsgType = 'Mail'
and ArchiveReason = 'Mailed'
and MsgSubType = 'Cancellation'
and convert(char(10), SystemTS, 101) = convert(char(10), getdate() , 101) 
and Confirmation like '%CRTR%'
and left(Param3, 1) <> 'I'
order by convert(char(10), msg.SystemTS, 101), Param3, EntityName

select PolicyNo, Name, Address, City, State, Zip, MailDate, CertifiedNo
from #tempCRTR
order by Sort, PolicyNo, Name

drop table #tempCRTR

END






GO
