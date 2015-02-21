USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[InstallmentsWithoutBatchMsgs2]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







/**
**/
-- ====================================================
-- Author:        
-- Create date: 
-- Description:   
-- ====================================================
CREATE PROCEDURE [dbo].[InstallmentsWithoutBatchMsgs2]
      -- Add the parameters for the stored procedure here
      (@pgmCode varchar(3), @programCode varchar(3),@runDate datetime, @minBalance numeric(9,2))
AS

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
		
	--DROP TABLE #installmentWithOutInvoices
	----  FOR TESTING PURPOSES
	--Declare @pgmCode varchar(3), @programCode varchar(3),@runDate datetime, @minBalance numeric(9,2)
	--set @pgmCode='209'
	--set @programCode='FLC'
	--set @runDate='5/15/2012'
	--set @minBalance='5'
	----  END FOR TESTING PURPOSES
	
	if @runDate is null
		Set @runDate=GETDATE()
		  --select CONVERT(varchar(10), @runDate, 20)
	CREATE TABLE #installmentWithOutInvoices (id INT,ProgramCode VARCHAR(3),
	PolicyNo  VARCHAR(15),InstallmentNum  VARCHAR(255),
	DueDate  DateTime, MsgID  VARCHAR(255),
	Balance numeric (9,2),ExcludeYN bit)

	Declare @cmd varchar(3000)

	set @cmd = 'insert into #installmentWithOutInvoices 
		select ROW_NUMBER() over (order by dbar.PolicyNo),dbar.ProgramCode,dbar.PolicyNo,dbar.InstallmentNum,i.DueDate,bm.MsgID,
		SUM(TransactionAmt),0 from PasCarrier..DirectBillAR dbar with (NOLOCK)
		join PasCarrier..Installment i with (NOLOCK)
			on dbar.PolicyNo=i.PolicyNo and dbar.InstallmentNum=i.InstallmentNum
		join PasCarrier..Policy p with (NOLOCK)
			on i.PolicyNo=p.PolicyNo and i.TermEffDate=p.TermEffDate and p.PolicyTransactionNum=1
		left outer join pgm' + @pgmCode + '..BatchMsg bm with (NOLOCK)
			on bm.PolicyID=dbar.PolicyNo collate database_default 
				and bm.TermEffDate=dbar.TermEffDate
				and bm.MsgType=''BATCH'' and (bm.MsgSubType=''Invoice'' or bm.MsgSubType=''CnxPend'')
				and ltrim(rtrim(bm.PAramXML.value(''(/clsParameterSet/InstallmentNum)[1]'', ''VARCHAR(20)'')))=dbar.InstallmentNum
		join PasCarrier..PolicyControl pctrl with (NOLOCK)
			on i.PolicyNo=pctrl.PolicyNo
		where p.PolicyExpDate>GETDATE()
		and dbar.ProgramCode = ''' + @programCode + '''

		and pctrl.PolicyStatusInd=''N''
		and i.FirstInstallmentYN=0
		and bm.MsgID is null

		and dbar.PolicyNo
		in (select PolicyNo from PasCarrier..DirectBillAR dbar2 with (NOLOCK)
			where dbar2.PolicyNo=dbar.PolicyNo and dbar2.InstallmentNum=dbar.InstallmentNum
				and dbar2.AddedDateT>=DATEADD(D,-1,''' + CONVERT(varchar(10), @runDate, 20) + '''))
		group by dbar.ProgramCode,dbar.PolicyNo,dbar.InstallmentNum,i.DueDate,bm.MsgID
		having sum(TransactionAmt)>0
		and SUM(TransactionAmt)> '+ cast(@minBalance as varchar(5))


	--select @cmd
	EXEC (@cmd)

	Declare @rowCount as int
	Declare @ctr as int
	set @ctr = 0
	select @rowcount=COUNT(*) from #installmentWithOutInvoices

	while @ctr<>@rowCount+1
	Begin
		set @ctr=@ctr+1
		Declare @PolicyNo as varchar(15)
		select @PolicyNo=PolicyNo from #installmentWithOutInvoices
			where id=@ctr
		--Declare @PolicyStatus as varchar(1)
		
		--select @PolicyStatus=PolicyStatusInd from PasCarrier..PolicyControl pc with (NOLOCK)
		--join #installmentWithOutInvoices i 
		--	on pc.PolicyNo=i.PolicyNo
		--where i.id=@ctr
		--if @PolicyStatus<>'N'
		--begin
		--	update #installmentWithOutInvoices
		--	set ExcludeYN=1
		--	where id=@ctr
		--	CONTINUE;
		--end 
		
		Declare @PayMethod as varchar(1)
		set @PayMethod = null
		select @PayMethod=PaymentMethodInd from PasCarrier..PolicyPaymentMethod ppm with (NOLOCK)
		join #installmentWithOutInvoices i 
			on ppm.PolicyNo=i.PolicyNo collate database_Default
		where i.id=@ctr
		if @PayMethod is not null and @PayMethod = 'E'
		begin
			update #installmentWithOutInvoices
			set ExcludeYN=1
			where id=@ctr
			CONTINUE;
		end 
		
		declare @PolicyBalance as decimal (15,2)
		select @PolicyBalance=SUM(TransactionAmt) from PasCarrier..DirectBillAR with (NOLOCK)
			where PolicyNo = @PolicyNo
		if @PolicyBalance=0
		begin
			update #installmentWithOutInvoices
			set ExcludeYN=1
			where id=@ctr
			CONTINUE;
		end
		
		declare @pendingPaymentCount as int
		select @pendingPaymentCount=COUNT(*) from Common..Payment pay with (NOLOCK)
			where PolicyNbr=@PolicyNo collate database_default
			and CancelledDate is null and PostedDate is null
			
		if @pendingPaymentCount>0
		begin
			update #installmentWithOutInvoices
			set ExcludeYN=1
			where id=@ctr
			CONTINUE;
		end
		
	end
	insert into common..InstallmentsWithoutBatchMsg
	select ProgramCode,PolicyNo,InstallmentNum,DueDate,Balance,null,null,'Data_Monitor',getdate()
	from  #installmentWithOutInvoices i
	where ExcludeYN=0
	and InstallmentNum not in (select InstallmentNum from Common..InstallmentsWithoutBatchMsg ins with (NOLOCK)
		where ins.InstallmentNum=i.InstallmentNum and ins.WorkedOn is null)
	DROP TABLE #installmentWithOutInvoices
	
END 
SET QUOTED_IDENTIFIER OFF








GO
