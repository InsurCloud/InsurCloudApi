USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[GetPolicy_Inv_Pay_Cancel]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- exec GetPolicy_Inv_Pay_Cancel 'AUTO', 'LA', 'LAC', '16C', '2012-09-01', '2012-11-01'


CREATE PROCEDURE [dbo].[GetPolicy_Inv_Pay_Cancel]
		@Product as varchar(15),
		@StateCode as varchar(2),
		@ProgramCode as varchar(3),
		@PayPlanCode as varchar(5),
		@StartDate as datetime,
		@EndDate as datetime

AS

if UPPER(@Product)='HOME'
set @Product='1'
if UPPER(@Product)='AUTO'
set @Product='2'

if UPPER(@StateCode)='AZ'
set @StateCode='02'
if UPPER(@StateCode)='AR'
set @StateCode='03'
if UPPER(@StateCode)='FL'
set @StateCode='09'
if UPPER(@StateCode)='LA'
set @StateCode='17'
if UPPER(@StateCode)='OK'
set @StateCode='35'
if UPPER(@StateCode)='TX'
set @StateCode='42'

---
--INVOICE RECORDS
--


declare @select as varchar(max)

set @Select ='
select 
		p.ProgramCode,
		p.PolicyNo,
       p.TermEffDate,
       p.PayPlanCode  AS PayPlan,
       ''Invoice''      AS Action,
       cast(inv.MailDate As datetime) AS ActionDate,
       null             AS PaymentAmt,
       null             AS CancelCode,
       null             AS CancelDesc,
       null             AS CancelFinalEffDate,
       null             AS CancelStatus,
       install.DueDate,
       img.FilePath
 from PasCarrier..Policy p
 join PasCarrier..Installment install
 on p.PolicyNo=install.PolicyNo and p.TermEffDate=install.TermEffDate
  
join pgm' + @Product + @StateCode + '..InvoiceDetails inv
on p.PolicyNo =inv.PolicyID collate database_default
and p.TermEffDate=inv.TermEffDate
and inv.InstallmentNum=install.InstallmentNum
join Imaging..Policy img
on inv.Imageid=img.ImageID
where 
p.PolicyTransactionNum=''1'' '
if UPPER(@ProgramCode)<>'ALL'
set @Select = @Select + ' and p.ProgramCode=''' + @ProgramCode + ''' '
if UPPER(@PayPlanCode)<>'ALL'
set @Select = @Select + ' and p.PayPlanCode=''' + @PayPlanCode + ''' '

Set @select = @Select + '  and inv.MailDate>=''' + cast(@StartDate as varchar(25)) + ''' and inv.MailDate<''' + cast(@EndDate as varchar(25)) + '''


union all

select 
		p.ProgramCode,
		p.PolicyNo,
       p.TermEffDate,
       p.PayPlanCode  AS PayPlanCode,
       ''Cancel''      AS ActionType,
       cast(canc.MailDate as datetime) AS ActionDate,
       null             AS PaymentAmt,
       canc.CancelCode             AS CancelCode,
       canc.CancelDesc             AS CancelDesc,
       canc.CnxFinalEffDate             AS CancelFinalEffDate,
       canc.Status             AS CancelStatus,
       null as DueDate,
       img.FilePath
 from PasCarrier..Policy p
  
join pgm' + @Product + @StateCode + '..CancelDetails canc
on p.PolicyNo =canc.PolicyID collate database_default
and p.TermEffDate=canc.TermEffDate
 
join Imaging..Policy img
on canc.ImageID=img.ImageID
where 
p.PolicyTransactionNum=''1'' '
if UPPER(@ProgramCode)<>'ALL'
set @Select = @Select + ' and p.ProgramCode=''' + @ProgramCode + ''' '
if UPPER(@PayPlanCode)<>'ALL'
set @Select = @Select + ' and p.PayPlanCode=''' + @PayPlanCode + ''' '

Set @select = @Select + ' and canc.MailDate>=''' + cast(@StartDate as varchar(25)) + ''' and canc.MailDate<''' + cast(@EndDate as varchar(25)) + '''



union all

---
--Payment RECORDS
--
select 		
		p.ProgramCode,
		p.PolicyNo,
       null as TermEffDate,
       p.PayPlanCode  AS PayPlan,
       ''Payment''      AS Action,
       cast(pay.paymentDate AS datetime) AS ActionDate,
       pay.PaymentAmt             AS PaymentAmt,
       null             AS CancelCode,
       null             AS CancelDesc,
       null            AS CancelFinalEffDate,
       null             AS CancelStatus,
       null as DueDate,
       null as FilePath
 from PasCarrier..Policy p
 join Common..Payment pay
 on  p.PolicyNo=pay.PolicyNbr collate database_default
 and pay.PaymentDate>=p.TermEffDate and pay.PaymentDate<p.PolicyExpDate
where 
p.PolicyTransactionNum=''1'' '
if UPPER(@ProgramCode)<>'ALL'
set @Select = @Select + ' and p.ProgramCode=''' + @ProgramCode + ''' '
if UPPER(@PayPlanCode)<>'ALL'
set @Select = @Select + ' and p.PayPlanCode=''' + @PayPlanCode + ''' '

Set @select = @Select + ' and pay.paymentDate>=''' + cast(@StartDate as varchar(25)) + ''' and pay.paymentDate<''' + cast(@EndDate as varchar(25)) + ''' 

order by PolicyNo,ActionDate'

print(@select)

exec(@select)



GO
