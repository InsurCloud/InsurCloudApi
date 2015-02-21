USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[UpdatePolicyLastPolicyUnitNumber_Mass]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[UpdatePolicyLastPolicyUnitNumber_Mass] 
--(
-- @DateRan	as datetime  
--)
AS
BEGIN
	SET NOCOUNT ON 
	declare @darnPolicies	as Table(
		CompanyCode			Varchar(10),
		ProgramCode			Varchar(10),
		PolicyNo			VARCHAR(20),
		TermEffDate			Datetime,
		PolicyTransaction	NUMERIC(10),
		LastUnitNum			NUMERIC(10),
		MaxUnitNum			NUMERIC(10)
	)
	insert into @darnPolicies
	SELECT 
		PU.CompanyCode,
		PU.ProgramCode,
		PU.PolicyNo, 
		PU.TermEffDate,
		PU.PolicyTransactionNum,
		PRS.LastPolicyUnitNum,
		MAX(PU.PolicyUnitNum) 
	FROM PasCarrier..PolicyRiskState PRS with (nolock)
	Inner Join PasCarrier..PolicyUnit PU with (nolock)
	On PRS.PolicyNo = PU.PolicyNo
	and  PRS.CompanyCode = PU.CompanyCode
	and  PRS.ProgramCode = PU.ProgramCode
	and  PRS.PolicyTransactionNum = PU.PolicyTransactionNum
	and  PRS.TermEffDate = PU.TermEffDate
	Group by PU.CompanyCode,PU.ProgramCode,PU.PolicyNo,PU.TermEffDate,PU.PolicyTransactionNum,PRS.LastPolicyUnitNum

	delete from @darnPolicies
	where LastUnitNum = MaxUnitNum

--	select @DateRan, * from @darnPolicies
	begin tran
	update PRS
	set LastPolicyUnitNum  = DP.MaxUnitNum
	from PasCarrier..PolicyRiskState PRS
	inner join @darnPolicies DP
	On PRS.CompanyCode = DP.CompanyCode collate SQL_Latin1_General_CP1_CI_AS 
	AND PRS.ProgramCode = DP.ProgramCode collate SQL_Latin1_General_CP1_CI_AS 
	AND PRS.PolicyNo = DP.PolicyNo collate SQL_Latin1_General_CP1_CI_AS 
	AND PRS.TermEffDate = DP.TermEffDate
	AND CAST(PRS.PolicyTransactionNum AS NUMERIC(10) )= DP.PolicyTransaction
	commit tran

insert into DataFixControl
select 'UpdatePolicyLastPolicyUnitNumber_Mass', getdate()

select null from DataFixControl
where 1=0


END 
/*
select * from INFORMATION_SCHEMA.COLUMNS
where COLUMN_NAME like '%UnitNum%'
and Table_name like 'Q%'

update PolicyRiskState
set LastPolicyUnitNum = 

update QuoteRiskState
set LastPolicyUnitNum = 

*/

GO
