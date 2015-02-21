USE [pgmMaster]
GO
/****** Object:  StoredProcedure [dbo].[CoverageIMCDevilIFAPLoad]    Script Date: 7/27/2014 4:24:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CoverageIMCDevilIFAPLoad]
AS 

/****** Object:  StoredProcedure [dbo].[PolicyIMCDevilIFAPLoad]    Script Date: 04/21/2009 13:37:45 ******/

-- exec dbo.[PolicyIMCDevilIFAPLoad]

update dbo.StageCoverageIMCDevilIFAP
set 
EffectiveDate = 
	case when len(EffectiveDate) < 6 then '0' + right(EffectiveDate, 5)
		else EffectiveDate end,
EntryDate = 
	case when len(EntryDate) < 6 then '0' + right(EntryDate, 5)
		else EntryDate end

Declare @EntryDate datetime
set @EntryDate = 
	case	
		when (select count(*) from dbo.VehicleCoverage where SourceSystemCode = 'IMC' and SourceInsertUserId = 'IFAP') = 0 then '1901-01-01'
		else (select max(SourceInsertDT) from dbo.VehicleCoverage where SourceSystemCode = 'IMC' AND SourceInsertUserId = 'IFAP')
	end

-- NEW 
IF OBJECT_ID('tempdb..#IceBoxCov')>0
    DROP TABLE #IceBoxCov

CREATE TABLE #IceBoxCov
    (
          PolicyNo					varchar(15)
		, PolicyEffDate				datetime
		, VehicleNo					int
        , PolicyTransactionNo		int
		, CovCode					numeric(10,0)
		, SegEffDate				datetime
		, SegExpDate				datetime
		, InactiveSegEffDate		datetime
		, InactiveSegExpDate		datetime
		, InactivatedTransNo		int
		, InactivatedBy				int
		, InternalSublineCode		varchar (3)
		, EndoTransCode				char (1)
		, DroppedCovSw				char (1)
        , WrittenPremiumAmt			money
		, TermPremiumAmt			money
		, SegWrittenPremiumAmt		money
		, AccountingPeriodNo		INT
		, SublineCode				VARCHAR(3)
		, RiskStateCode				VARCHAR(2)
--		, TerritoryCode				VARCHAR(3)
		, FullyEarnedFlag			BIT
		, ImmediateEarnedFlag		BIT
		, SubjectToReinsuranceFlag	BIT
		, AgentCommissionRate		DECIMAL(21,6)
		, AgentCommissionAmt		MONEY
		, SourceInsertUserId		VARCHAR(64)
		, SourceInsertDT			DATETIME
		, SourceSystemCode			cHAR(3)
    )

CREATE INDEX IX_#IceBoxCov ON #IceBoxCov (PolicyNo, PolicyEffDate, VehicleNo, PolicyTransactionNo, CovCode, SegEffDate, SegExpDate)
-- END NEW 
IF OBJECT_ID('tempdb..#IceBoxVCovSeg')>0
    DROP TABLE #IceBoxVCovSeg

CREATE TABLE #IceBoxVCovSeg
    (
          PolicyNo					varchar(31)
		, PolicyEffDate				datetime
		, VehicleNo					int
        , PolicyTransactionNo		int
		, CovCode					numeric(10,0)
		, SegEffDate				datetime
		, SegExpDate				datetime
		, InactivatedBy				int
		, InternalSublineCode		varchar (3)
        , WrittenPremiumAmt			money
		, TermPremiumAmt			money
		, SegWrittenPremiumAmt		money
		, AccountingPeriodNo		INT
		, SublineCode				VARCHAR(3)
		, RiskStateCode				VARCHAR(2)
--		, TerritoryCode				VARCHAR(3)
		, FullyEarnedFlag			BIT
		, ImmediateEarnedFlag		BIT
		, SubjectToReinsuranceFlag	BIT
		, AgentCommissionRate		DECIMAL(21,6)
		, AgentCommissionAmt		MONEY
		, FrontingCommissionAmt		MONEY
		, SourceInsertUserId		VARCHAR(64)
		, SourceInsertDT			DATETIME
		, SourceSystemCode			CHAR(3)
		, InsertDT					DATETIME
		, InactiveSegExpDate		DATETIME
		, DroppedCovSw				char (1)
		, EndoTransCode				char (1)
    )

CREATE INDEX IX_#IceBoxVCovSeg ON #IceBoxVCovSeg (PolicyNo, PolicyEffDate, VehicleNo, PolicyTransactionNo, CovCode, SegEffDate, SegExpDate)
-- END NEW 


IF OBJECT_ID('tempdb..#IceBoxSeq')>0
    DROP TABLE #IceBoxSeq

CREATE TABLE #IceBoxSeq
    (
          PolicyNo					VARCHAR(15)
		, PolicyEffDate				DATETIME
        , PolicyTransactionNo		INT
		, VehicleNo					INT
		, InternalSublineCode		VARCHAR(3)
)
	
CREATE INDEX IX_#IceBoxSeq ON #IceBoxSeq (PolicyNo, PolicyEffDate, PolicyTransactionNo, VehicleNo, InternalSublineCode)


IF OBJECT_ID('tempdb..#TempVehCov')>0
    DROP TABLE #TempVehCov

CREATE TABLE #TempVehCov
(
	PolicyNo				varchar(31) 
	,PolicyEffDate			smalldatetime 
	,VehicleNo				int  
	,PolicyTransactionNo	int 
	,CovCode				numeric(10, 0)  
	,SegEffDate				smalldatetime 
	,SegExpDate				smalldatetime 
	,InactivatedBy			int 
	,InternalSublineCode	varchar(3) 
	,WrittenPremiumAmt		money 
	,TermPremiumAmt			money 
	,SegWrittenPremiumAmt	money
	,AccountingPeriodNo		int  
	,SublineCode			varchar(3) 
	,RiskStateCode			varchar(2) 
	,FullyEarnedFlag		bit
	,ImmediateEarnedFlag	bit
	,SubjectToReinsuranceFlag bit 
	,AgentCommissionRate	decimal(21, 6) 
	,AgentCommissionAmt		money  
	,FrontingCommissionAmt  money
	,SourceInsertUserId		varchar(64) 
	,SourceInsertDT			datetime 
	,SourceSystemCode		char(3) 
	,InsertDT				datetime
	,InactiveSegExpDate		datetime
	,DroppedCovSw			varchar(1)
	,EndoTransCode			varchar(1)

)

CREATE INDEX IX_#TempVehCov ON #TempVehCov (PolicyNo, PolicyEffDate, PolicyTransactionNo, VehicleNo, InternalSublineCode)

-- added 04-25-2009
IF OBJECT_ID('tempdb..#IceBoxVCovSegBU')>0
    DROP TABLE #IceBoxVCovSegBU

CREATE TABLE #IceBoxVCovSegBU
    (
          PolicyNo					varchar(31)
		, PolicyEffDate				datetime
		, VehicleNo					int
        , PolicyTransactionNo		int
		, CovCode					numeric(10,0)
		, SegEffDate				datetime
		, SegExpDate				datetime
		, InactivatedBy				int
		, InternalSublineCode		varchar (3)
        , WrittenPremiumAmt			money
		, TermPremiumAmt			money
		, SegWrittenPremiumAmt		money
		, AccountingPeriodNo		INT
		, SublineCode				VARCHAR(3)
		, RiskStateCode				VARCHAR(2)
--		, TerritoryCode				VARCHAR(3)
		, FullyEarnedFlag			BIT
		, ImmediateEarnedFlag		BIT
		, SubjectToReinsuranceFlag	BIT
		, AgentCommissionRate		DECIMAL(21,6)
		, AgentCommissionAmt		MONEY
		, FrontingCommissionAmt		MONEY
		, SourceInsertUserId		VARCHAR(64)
		, SourceInsertDT			DATETIME
		, SourceSystemCode			CHAR(3)
		, InsertDT					DATETIME
		, InactiveSegExpDate		DATETIME
		, DroppedCovSw				char (1)
		, EndoTransCode				char (1)
    )

CREATE INDEX IX_#IceBoxVCovSegBU ON #IceBoxVCovSegBU (PolicyNo, PolicyEffDate, VehicleNo, PolicyTransactionNo, CovCode, SegEffDate, SegExpDate)
-- END NEW 


-- Begin:  Add to IceBoxCov - use to collect data before putting into VehicleCov
Insert into #IceBoxCov
	select 
	 P.PolicyNo AS PolicyNo 
	, P.PolicyEffDate as PolicyEffDate
	, CAST(AutoNumber AS NUMERIC) as VehicleNo  
--	, CAST(ISNULL(V.VehicleNo,0) as numeric) as VehicleNo
	, cAST(SequenceNumber AS NUMERIC) as PolicyTransactionNo
	, X.CovCode as CovCode 
	,  P.EndorsementEffDate  AS SegEffDate
	,  Case when EndoTransCode in ('A', 'D', 'F', 'P', 'S') THEN P.EndorsementEffDate
			else P.PolicyExpDate END AS SeqExpDate
	, P.PolicyEffDate AS InactiveSegEffDate
	, P.PolicyExpDate AS InactiveSegExpDate
	, 0 as InactivatedTransNo
	, 0 as InactivatedBy
	, X.CovGroup as InternalSublineCode	
	, ISNULL(EndoTransCode,' ') AS EndoTransCode
	, ISNULL(DroppedCovSw,' ') as DroppedCovSw
	, case when EndoTransCode in ('B', 'H', 'N', 'O', 'T', 'U', 'V', 'X', 'Y', 'Z') THEN 0
 		ELSE  TransactionPrem end AS WrittenPremiumAmt
	, case when EndoTransCode in ('B', 'H', 'N', 'O', 'T', 'U', 'V', 'X', 'Y', 'Z') THEN 0
		   when cast(WrittenPremium as numeric) < 0 then  cast(WrittenPremium as numeric) * -1
 		ELSE  WrittenPremium end AS TermPremiumAmt
--	, ROUND((WrittenPremium/datediff(dd, P.PolicyEffDate, P.PolicyExpDate)) * datediff(dd,C.SegEffDate, CS.SegEffDate) , 2)  as SegWrittenPremiumAmt  -- calculate
--	, CASE WHEN EndoTransCode in (' ', 'A', 'E', 'C', 'D', 'F', 'P', 'S', 'R', 'W') THEN cast(TransactionPrem as money)
--		ELSE 0 END AS SegWrittenPremiumAmt
	, 0 as SegWrittenPremiumAmt
	, P.AccountingPeriodNo as AccountingPeriodNo	
	, CASE	
		WHEN CoverageType = 'BI  ' THEN '501'
		WHEN CoverageType = 'COLL' THEN '531'
		WHEN CoverageType = 'COMP' THEN '530'
		WHEN CoverageType = 'MED ' THEN '504'
		WHEN CoverageType = 'PD  ' THEN '502'
		WHEN CoverageType = 'PE  ' THEN '530'
		WHEN CoverageType = 'RENT' THEN '530'
		WHEN CoverageType = 'TOW ' THEN 'REN'
		WHEN CoverageType = 'UMBI' THEN '505'
		WHEN CoverageType = 'UMPD' THEN '506'
		WHEN CoverageType = 'UMWP' THEN '506'
		ELSE left(CoverageType,3) end as SublineCode		
	, 'LA' as RiskStateCode
	, 0 as FullyEarnedFlag
	, 0 as ImmediateEarnedFlag
	, 0 as SubjectToReinsuranceFlag
	, 0 as AgentCommissionRate
	, 0 as AgentCommissionAmt
	, 'IFAP' as SourceInsertUserID
	, P.SourceInsertDT as SourceInsertDT
	, 'IMC' as SourceSystemCode
FROM dbo.StageCoverageIMCDevilIFAP A	
	INNER JOIN ICEBox..Policy P
	on substring(P.PolicyNo, 5, 6) = A.PolicyNumber 
	and P.PolicyTransactionNo = A.SequenceNumber
LEFT OUTER JOIN ICEBox..Vehicle V
	on substring(V.PolicyNo, 5, 6) = A.PolicyNumber 
	and V.PolicyTransactionNo = A.SequenceNumber
	and V.VehicleNo = A.AutoNumber
	AND V.SourceInsertUserId = 'IFAP'
INNER JOIN ICEBox..vwPgmCovDetail_Master_XRefSystemCodes X
	on A.CoverageCode = X.SourceCovCode
--	and PPA.CovCode = X.CovCode
	AND P.ProgramCode = 'IFAP'
WHERE  -- DroppedCovSw <> 'N'
 	   ((EndoTransCode NOT IN  ('B', 'E', 'H', 'O', 'N', 'T', 'U', 'X', 'Y', 'Z'))
		OR (EndoTransCode = 'E' AND WrittenAmt <>0 and FeeAmt <> 0 ))
		-- OR (EndoTransCode = 'E' AND TransactionPrem <> 0))
	AND P.SourceInsertDt > @ENTRYDATE
	    and (EndoTransCode <> 'E' OR (EndoTransCode = 'E' and P.EndorsementEffDate <> P.PolicyExpDate))
    --   and P.PolicyNo in ('IFT800283-9-6')

-- End:  Main IceBoxCov insert

-- Begin:  Get Sequences for figuring InactivatedBy sequence
Insert into #IceBoxSeq
	select distinct V.PolicyNo, V.PolicyEffDate, V.PolicyTransactionNo, V.VehicleNo, V.InternalSublineCode
	From VehicleCoverage V
		inner join #IceBoxCov I
		on V.PolicyNo = I.PolicyNo
		and V.PolicyEffDate = I.PolicyEffDate
		and V.VehicleNo = I.VehicleNo
		and V.InternalSublineCode = I.InternalSublineCode
		and V.SourceInsertUserId = I.SourceInsertUserId	
		and V.SourceInsertUserId = 'IFAP'

Insert into #IceBoxSeq select distinct PolicyNo, PolicyEffDate, PolicyTransactionNo , VehicleNo, InternalSublineCode
	From  #IceBoxCov 
-- END:  Sequences

-- Select * from #IceBoxSeq

-- SELECT * FROM #IceBoxCov order by PolicyNo, PolicyEffDate, PolicyTransactionNo

-- may have a problem here by adding VehicleNo to IceBoxSeq  04-02-09 (due TO DROPPED VEHICLES
Update #IceBoxCov
	SET  InactivatedTransNo = P.PolicyTransactionNo, SegEffDate = P.PolicyEffDate, InactiveSegEffDate = P.EndorsementEffDate, InactiveSegExpDate = P.PolicyExpDate
	From #IceBoxCov I
		INNER JOIN Policy P
		on I.PolicyNo = P.PolicyNo
		and I.PolicyEffDate = P.PolicyEffDate
	where I.EndoTransCode in ('A', 'D', 'F', 'P', 'S')
			and  P.PolicyTransactionNo < I.PolicyTransactionNo 
		    and  P.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #IceBoxSeq t
				where P.PolicyNo = t.PolicyNo
					and P.PolicyEffDate = t.PolicyEffDate
					and t.PolicyTransactionNo < I.PolicyTransactionNo)

Update #IceBoxCov
	SET  InactivatedTransNo = P.PolicyTransactionNo, SegEffDate = P.PolicyEffDate, InactiveSegEffDate = P.EndorsementEffDate, InactiveSegExpDate = P.PolicyExpDate
	From #IceBoxCov I
		INNER JOIN Policy P
		on I.PolicyNo = P.PolicyNo
		and I.PolicyEffDate = P.PolicyEffDate
	where I.EndoTransCode = 'R'
			and  P.PolicyTransactionNo < I.PolicyTransactionNo 
		    and  P.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #IceBoxSeq t
				where P.PolicyNo = t.PolicyNo
					and P.PolicyEffDate = t.PolicyEffDate
					and t.PolicyTransactionNo < I.PolicyTransactionNo)



-- NEED CODE TO GET LAST SegEffDate from last Segment for Canc & Reinstate if one 04-14-09
-- 1st attempt - check on VehicleCov - may do a temp VehicleCov - cause may need to use current SegRecords
Update #IceBoxCov
	set SegEffdate = V.SegExpDate
		from #IceBoxCov I
	INNER JOIN VehicleCoverage V
		on I.PolicyNo = V.PolicyNo
		and I.PolicyEffDate = V.PolicyEffDate
		and I.VehicleNo = V.VehicleNo
		and I.CovCode = V.CovCode
		and I.PolicyTransactionNo > V.PolicyTransactionNo
	INNER JOIN Policy P 
		on P.PolicyNo = V.PolicyNo
		and P.PolicyEffDate = V.PolicyEffDate
		and P.PolicyTransactionNo = V.PolicyTransactionNo
	where V.InactivatedBy = 0
		AND p.PolicyActionCode = 'E'	and V.SegExpDate < I.SEgExpDate	-- 2009-08-31 & 2009-09-02 ADDED THIS LINE and joining of Policy 
		AND I.EndoTransCode NOT IN  ('A', 'D', 'F', 'P', 'S')	-- 2009-08-31 flat cancel should have EffDate as SegEffDate
		AND V.PolicyTransactionNo = (SELECT MAX(t.PolicyTransactionNo)
			from VehicleCoverage t
			where t.PolicyNo = V.PolicyNo
				and t.PolicyEffDate = V.PolicyEffDate
				and t.PolicyTransactionNo < I.PolicyTransactionNo)


-- 2009-09-01 added whole section
Update #IceBoxCov
	set SegEffdate = V.SegExpDate
		from #IceBoxCov I
	INNER JOIN VehicleCoverage V
		on I.PolicyNo = V.PolicyNo
		AND I.PolicyEffDate = V.PolicyEffDate
		AND I.VehicleNo = V.VehicleNo
		AND I.CovCode = V.CovCode
		AND I.PolicyTransactionNo > V.PolicyTransactionNo
	INNER JOIN Policy P 
		on P.PolicyNo = V.PolicyNo
		and P.PolicyEffDate = V.PolicyEffDate
		and P.PolicyTransactionNo = V.PolicyTransactionNo
	where V.InactivatedBy = 0
		AND p.PolicyActionCode = 'E' AND V.SegExpDate <= I.SegExpDate -- 2009-08-31 ADDED THIS LINE and joining of Policy 
		AND I.EndoTransCode  IN  ('D', 'P', 'S' )	-- 2009-08-31 flat cancel should have EffDate as SegEffDate
		AND V.PolicyTransactionNo = (SELECT MAX(t.PolicyTransactionNo)
			from VehicleCoverage t
			where t.PolicyNo = V.PolicyNo
				and t.PolicyEffDate = V.PolicyEffDate
				and t.PolicyTransactionNo < I.PolicyTransactionNo)


Update #IceBoxCov
	SET  InactivatedTransNo = P.PolicyTransactionNo
	From #IceBoxCov I
		INNER JOIN Policy P
		on I.PolicyNo = P.PolicyNo
		and I.PolicyEffDate = P.PolicyEffDate
	where I.EndoTransCode = 'E'
			and  P.PolicyTransactionNo < I.PolicyTransactionNo 
		    and  P.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #IceBoxSeq t
				where P.PolicyNo = t.PolicyNo
					and P.PolicyEffDate = t.PolicyEffDate
					and t.PolicyTransactionNo < I.PolicyTransactionNo)

-- Calc SegPremium
--Update #IceBoxCov
--	SET SegWrittenPremiumAmt = isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd,SegEffDate, SegExpDate) , 0) ,0)  -- calculate
Update #IceBoxCov
	SET SegWrittenPremiumAmt = 
		(CASE WHEN EndoTransCode = 'S' THEN 
		ISNULL(ROUND((TermPremiumAmt - ((TermPremiumAmt -  
		(isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd, SegEffDate, SegExpDate) , 0) ,0))) * 90 ) / 100) , 0) , 0)
		ELSE  isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd,SegEffDate, SegExpDate) , 0) ,0)
		END)


 -- SELECT * FROM #IceBoxCov order by PolicyNo, PolicyTransactionNo

-- INSERT INTO VehicleCoverage before setting Inactivate switch - changed to #TempVehCov
-- WILL need to insert #TempVehCov into VehicleCoverge 04-14-09 -- ATTENTION
Insert into #TempVehCov
	select 
	a.PolicyNo as PolicyNo
	, a.PolicyEffdate as PolicyEffDate
	, a.VehicleNo as VehicleNo
	, a.PolicyTransactionNo as PolicyTransactionNo
	, a.CovCode as CovCode
	, a.SegEffDate as SegEffDate
	, a.SegExpDate as SegExpDate
	, a.InactivatedBy as InactivatedBy
	, a.InternalSublineCode as InternalSublineCode
	, a.WrittenPremiumAmt as WrittenPremiumAmt
	, a.TermPremiumAmt as TermPremiumAmt
	, a.SegWrittenPremiumAmt as SegWrittenPremiumAmt
	, a.AccountingPeriodNo as AccountingPeriodNo
	, a.SublineCode as SublineCode
	, a.RiskStateCode as RiskStateCode
	, a.FullyEarnedFlag as FullyEarnedFlag
	, a.ImmediateEarnedFlag as ImmediateEarnedFlag
	, a.SubjectToReinsuranceFlag as SubjectToReinsuranceFlag
	, a.AgentCommissionRate as AgentCommissionRate
	, a.AgentCommissionAmt as AgentCommissionAmt
	, NULL AS FrontingCommissionAmt
	, a.SourceInsertUserId as SourceInsertUserId
	, a.SourceInsertDT as SourceInsertDT
	, a.SourceSystemCode as SourceSystemCode
	, GETDATE() AS InsertDT 
	, a.InactiveSegExpDate as InactiveSegExpDate 
	, a.DroppedCovSw as DroppedCovSw
	, a.EndoTransCode as EndoTransCode
from #IceBoxCov A 

-- below works until add segmented sequence - then need to distinguish between or add segmented seq after  this.
-- If seq not in #TempVehCov - then need to update Vehicle Coverage
Update #TempVehCov
	set InactivatedBy = I.PolicyTransactionNo
	from #TempVehCov V
		INNER JOIN #IceBoxCov I
		ON V.PolicyNo = I.PolicyNo
--		and V.VehicleNo = I.VehicleNo  -- 04/23/2009 NOT MARKING PRIOR VEH 2 CAUSE DROPPED ON CUR SEQ
		and V.PolicyEffdate = I.PolicyEffdate
		and V.PolicyTransactionNo = I.InactivatedTransNo 
	where I.EndoTransCode IN ('E', 'A', 'D', 'F', 'P', 'R', 'S')
			and  V.PolicyTransactionNo < I.PolicyTransactionNo 
		    and  V.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #IceBoxSeq t
				where V.PolicyNo = t.PolicyNo
					and V.PolicyEffDate = t.PolicyEffDate
					and t.PolicyTransactionNo < I.PolicyTransactionNo
					 )

-- PRINT
--SELECT * FROM #IceBoxCov
--select * from #TempVehCov

-- update in VehicleCoverage if record to update is not in #TempVehCov
-- can I inactivate Endos, cancels and reinstates at same time?  04-14-09
-- are my seg eff dates correct at this time?? it makes a difference!!!
Update VehicleCoverage
	set InactivatedBy = I.PolicyTransactionNo
	from VehicleCoverage V
		INNER JOIN #IceBoxCov I
		ON V.PolicyNo = I.PolicyNo
		and V.VehicleNo = I.VehicleNo
		and V.PolicyEffdate = I.PolicyEffdate
		and V.PolicyTransactionNo = I.InactivatedTransNo 
		and V.InactivatedBy = 0
	where I.EndoTransCode IN ('E', 'A', 'D', 'F', 'P', 'R', 'S')
			and  V.PolicyTransactionNo < I.PolicyTransactionNo 
		    and  V.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #IceBoxSeq t
				where V.PolicyNo = t.PolicyNo
					and V.PolicyEffDate = t.PolicyEffDate
					and t.PolicyTransactionNo < I.PolicyTransactionNo
					and I.SegExpDate between V.SegEffDate and V.SegExpDate
				
)


-- set SegEffDate of Reinstate = SeqEffDate of Prior Cancel
-- need to test
	
--Update VehicleCoverage
--	SET  SegEffDate = V.SegEffDate
--	From VehicleCoverage V
--		INNER JOIN #IceBoxCov I
--			ON  V.PolicyNo = I.PolicyNo
--			and V.VehicleNo = I.VehicleNo
--			and V.InternalSublineCode = I.InternalSublineCode
--			and V.PolicyEffdate = I.PolicyEffdate
--			AND I.EndoTransCode = 'R'
--			and  PolicyTransactionNo = V.InactivatedBy 
--		WHERE PolicyNo = V.PolicyNo
--			and PolicyEffDate = V.PolicyEffDate

		    

-- now do segmented row for endorsement  ?? huh ?? not getting to VehicleCov ** check 802473 vs 802544
INSERT INTO #IceBoxVCovSeg 
	SELECT  V.*  -- , I.InactiveSegExpDate, I.DroppedCovSw, I.EndoTransCode   
	FROM #TempVehCov V
		INNER JOIN #TempVehCov X
		on  X.PolicyNo = V.PolicyNo
			and X.PolicyEffDate = V.PolicyEffDate
--			and X.DistinctVehNo = V.DistinctVehNo		
	where X.EndoTransCode = 'E'
		and X.PolicyTransactionNo = V.InactivatedBy
--	and not exists
--		(select * from #IceBoxVCovSeg where DistinctVehNo = V.DistinctVehNo)   

-- delete from #IceBoxVCovSeg


INSERT INTO #IceBoxVCovSegBU SELECT * FROM #IceBoxVCovSeg
TRUNCATE TABLE #IceBoxVCovSeg
INSERT INTO #IceBoxVCovSeg SELECT DISTINCT * FROM #IceBoxVCovSegBU

-- PRINT
--SELECT * FROM #IceBoxVCovSeg
--SELECT * from #TempVehCov

-- 04/23/2009 NOT GETTING CORRECT SegEffDate PRIOR SEQUENCE HAS NOT BEEN UPDATED YET !!!!!
-- 04/23/2009 do I need this code in both places?? - moving it below - will check dele'ing this 1 later
Update #IceBoxVCovSeg
	SET    SegEffDate = V.SegEffDate
	From #IceBoxVCovSeg I
		INNER JOIN #TempVehCov V
		on I.PolicyNo = V.PolicyNo
		and I.PolicyEffDate = V.PolicyEffDate
		AND I.PolicyTransactionNo > V.PolicyTransactionNo
		and I.VehicleNo = V.VehicleNo
		and I.CovCode = V.CovCode		and I.VehicleNo = V.VehicleNo
	where -- V.InactivatedBy <> 0 -- 04-23-2009 - think should be looking at regular record not Seg Record
		      V.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #TempVehCov t
				where V.PolicyNo = t.PolicyNo
					and V.PolicyEffDate = t.PolicyEffDate)
--					and t.PolicyTransactionNo < .PolicyTransactionNo)
--	where V.InactivatedBy <> 0 
--		    and  V.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
--				from VehicleCoverage t
--				where V.PolicyNo = t.PolicyNo
--					and V.PolicyEffDate = t.PolicyEffDate)

-- PRINT
--SELECT * FROM #IceBoxVCovSeg
--SELECT * from #TempVehCov

-- 04/15/2009  Assume if InactivatedTransNo = 0 then deleted transaction - DELETE - may have to fine tune this later
DELETE #IceBoxVCovSeg WHERE InactivatedBy = 0 

-- then
UPDATE #IceBoxVCovSeg SET PolicyTransactionNo = InactivatedBy
-- PRINT
 -- select * from #TempVehCov

-- not working for dropped car - dele vehicle No check - was working but now not working 04/23/2009
-- 04-23-2009 working seq  of 300157
Update #IceBoxVCovSeg
	SET    SegExpDate = V.SegEffDate, AccountingPeriodNo = V.AccountingPeriodNo
	From #IceBoxVCovSeg I
		INNER JOIN #TempVehCov V
		on I.PolicyNo = V.PolicyNo
		and I.PolicyEffDate = V.PolicyEffDate
		AND I.InactivatedBy = V.PolicyTransactionNo
--		and I.VehicleNo = V.VehicleNo -- 04-23-2009
--		and I.CovCode = V.CovCode				-- 04-28-2009 remarked  808467-7

--  04-25-2009  --  provide for dropped coverage
INSERT INTO #IceBoxVCovSegBU SELECT * FROM #IceBoxVCovSeg
--  Added 04-24-2009 to get correct SegExpDate for Dropped Records
Update #IceBoxVCovSeg
	SET    SegExpDate = BU.SegExpDate
	FROM #IceBoxVCovSeg I
	INNER JOIN #IceBoxVCovSegBU BU
	ON 	 I.PolicyNo = BU.PolicyNo
		and I.PolicyEffDate = BU.PolicyEffDate
		AND I.InactivatedBy = BU.InactivatedBy
	where  I.DroppedCovSw = 'N'
		AND I.SegExpDate > BU.SegExpDate


--  04-25-2009 end provide for dropped coverage

-- PRINT 04-23-2009
-- select * from #IceBoxVCovSeg
-- SELECT * FROM #TempVehCov

UPDATE #IceBoxVCovSeg SET InactivatedBy = 0 

-- get correct SegEffDate for cancel seq - will be effdate or SegEFfDate of the last TransNo that is not Inactivated 
-- 04-14-09 THIS IS NOT WORKING WHY?????  (Wrong Letter using T s/b I)
-- 04-23-2009 WILL MOVING THIS SECTION UP MAKE SegEff work for Seg records?  -  or is it a maze - which comes 1st the chicken or the egg.
UPDATE #TempVehCov
	set SegEffDate = I.SegExpDate
	from #TempVehCov T
	INNER JOIN #IceBoxVCovSeg I
		on T.PolicyNo = I.PolicyNo
			and T.PolicyEffDate = I.PolicyEffDate
--			and T.VehicleNo = I.VehicleNo
--			and T.CovCode = I.CovCode
	INNER JOIN #ICEBOXCOV B
		on 	T.PolicyNo = B.PolicyNo
			and T.PolicyEffDate = B.PolicyEffDate
			and T.VehicleNo = B.VehicleNo
			and T.CovCode = B.CovCode
			AND T.PolicyTransactionNo = B.PolicyTransactionNo
		where B.EndoTransCode IN ('A', 'D', 'F', 'P', 'R', 'S')
			AND i.InactivatedBy = 0
			AND I.SegExpDate < T.SegExpDate  --  2009-08-31 TO FIX SegEffDate
			and  T.PolicyTransactionNo > I.PolicyTransactionNo
			and I.PolicyTransactionNo = ( SELECT MAX(X.PolicyTransactionNo)
			from #IceBoxVCovSeg X
			where X.PolicyNo = I.PolicyNo
			and X.PolicyEffDate = I.PolicyEffDate
--			and X.VehicleNo = I.VehicleNo
--			and X.CovCode = I.CovCode
			and X.PolicyTransactionNo < T.PolicyTransactionNo)
-- PRINT 04-23-2009 -- CHECK Canc SegDate
-- SELECT * from #IceBoxVCovSeg
-- SELECT * FROM #TempVehCov


-- PRINT IT 
   SELECT * FROM #TempVehCov  

-- 04-23-2009- this section moved from above but not deleted above  - corrected SegEff but now SegExp is wrong again
Update #IceBoxVCovSeg
	SET    SegEffDate = V.SegEffDate
	From #IceBoxVCovSeg I
		INNER JOIN #TempVehCov V
		on I.PolicyNo = V.PolicyNo
		and I.PolicyEffDate = V.PolicyEffDate
		AND I.PolicyTransactionNo > V.PolicyTransactionNo
		and I.VehicleNo = V.VehicleNo
		and I.CovCode = V.CovCode		and I.VehicleNo = V.VehicleNo
	where -- V.InactivatedBy <> 0 -- 04-23-2009 - think should be looking at regular record not Seg Record
		      V.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
				from #TempVehCov t
				where V.PolicyNo = t.PolicyNo
					and V.PolicyEffDate = t.PolicyEffDate
					and I.PolicyTransactionNo > t.PolicyTransactionNo)

-- TEST
--SELECT I.PolicyTransactionNo, V.PolicyTransactionNo,V.SegEffDate
--From #IceBoxVCovSeg I
--		INNER JOIN #TempVehCov V
--		on I.PolicyNo = V.PolicyNo
--		and I.PolicyEffDate = V.PolicyEffDate
--		AND I.PolicyTransactionNo > V.PolicyTransactionNo
--		and I.VehicleNo = V.VehicleNo
--		and I.CovCode = V.CovCode	
--	where -- V.InactivatedBy <> 0 -- 04-23-2009 - think should be looking at regular record not Seg Record
--	--		 I.PolicyTransactionNo > V.PolicyTransactionNo
--		      V.PolicyTransactionNo = (select MAX(t.PolicyTransactionNo)
--				from #TempVehCov t
--				where V.PolicyNo = t.PolicyNo
--					and V.PolicyEffDate = t.PolicyEffDate
--					and I.PolicyTransactionNo > t.PolicyTransactionNo)
-- PRINT IT 
--   SELECT * FROM #TempVehCov  
--   SELECT * FROM #IceBoxVCovSeg
-- stop here to see if Canc SEG eff date is correct

--Update #TempVehCov
--	SET SegWrittenPremiumAmt = isnull(ROUND((T.TermPremiumAmt/datediff(dd, T.PolicyEffDate, B.InactiveSegExpDate)) * datediff(dd,T.SegEffDate, T.SegExpDate) , 0) ,0)  -- calculate
--From #TempVehCov T
--	INNER JOIN #ICEBOXCOV B
--	ON T.PolicyNo = B.PolicyNo
--		and T.PolicyEffDate = B.PolicyEffDate

-- Calc SegPremium
Update #TempVehCov
	SET SegWrittenPremiumAmt = 
		(CASE WHEN T.EndoTransCode = 'S' THEN 
		ISNULL(ROUND((T.TermPremiumAmt - ((T.TermPremiumAmt -  
		(isnull(ROUND((T.TermPremiumAmt/datediff(dd, T.PolicyEffDate, B.InactiveSegExpDate)) * datediff(dd, T.SegEffDate, T.SegExpDate) , 0) ,0))) * 90 ) / 100) , 0) , 0)
--			T.TermPremiumAmt - ((T.TermPremiumAmt -  (isnull(ROUND((T.TermPremiumAmt/datediff(dd, T.PolicyEffDate, B.InactiveSegExpDate)) * datediff(dd, T.SegEffDate, T.SegExpDate) , 0) ,0))) * 90 ) / 100
		ELSE  isnull(ROUND((T.TermPremiumAmt/datediff(dd, T.PolicyEffDate, B.InactiveSegExpDate)) * datediff(dd, T.SegEffDate, T.SegExpDate) , 0) ,0)
			END)
From #TempVehCov T
	INNER JOIN #ICEBOXCOV B
	ON T.PolicyNo = B.PolicyNo
		and T.PolicyEffDate = B.PolicyEffDate
		AND T.PolicyTransactionNo = B.PolicyTransactionNo


-- PRINT
-- also have to check VehicleCoverage for correct SegEffDate	
-- SELECT * FROM #IceBoxVCovSeg where PolicyNo = 'AIP0300056-6'
--   SELECT * FROM #TempVehCov  
--  SELECT * FROM #ICEBOXCOV where PolicyNo = 'AIP0300056-6'



	
--Update #IceBoxVCovSeg
--	SET SegWrittenPremiumAmt = isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd,SegEffDate, SegExpDate) , 0) ,0)  -- calculate
-- Calc SegPremium
Update #IceBoxVCovSeg
	SET SegWrittenPremiumAmt = 
		(CASE WHEN EndoTransCode = 'S' THEN 
		ISNULL(ROUND((TermPremiumAmt - ((TermPremiumAmt -  
		(isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd, SegEffDate, SegExpDate) , 0) ,0))) * 90 ) / 100) , 0) , 0)
--		TermPremiumAmt - ((TermPremiumAmt -  (isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd, SegEffDate, SegExpDate) , 0) ,0))) * 90 ) / 100
		ELSE  isnull(ROUND((TermPremiumAmt/datediff(dd, PolicyEffDate, InactiveSegExpDate)) * datediff(dd,SegEffDate, SegExpDate) , 0) ,0)
			END)

-- 2009-05-11  TO BALANCE WrittenPremium  --
Update #IceBoxVCovSeg
	SET WrittenPremiumAmt = 0

-- 2009-05-11  SegWrittenPrem - must be -0- on dropped coverage
-- 2009-05-12 -0- TermPremiumAmt  - 
UPDATE #TempVehCov 
	SET SegWrittenPremiumAmt = 0, TermPremiumAmt = 0 WHERE DroppedCovSw = 'N'


 
UPDATE #IceBoxVCovSeg
	SET AgentCommissionRate = ISNULL(a.AgentCommissionRate,0), AgentCommissionAmt = ISNULL(round(c.WrittenPremiumAmt * a.AgentCommissionRate,2),0)
from  #IceBoxVCovSeg c
	INNER JOIN ICEBox..Policy p
		on c.PolicyNo = p.PolicyNo
		AND c.PolicyEffDate = p.PolicyEffDate
		AND c.PolicyTransactionNo = p.PolicyTransactionNo
		AND c.SourceSystemCode = p.SourceSystemCode
	INNER JOIN ICEBox..pgmAgentCommission a
		on p.ProgramCode = a.ProgramCode
		AND a.AgentCode = p.AgentCode
		and a.CommissionEffDate <= c.PolicyEffDate
		and a.CommissionEffDate = (select MAX(x.CommissionEffDate)
			FROM ICEBox..pgmAgentCommission x
				where x.ProgramCode = a.ProgramCode
					AND a.AgentCode = x.AgentCode
					AND x.CommissionEffDate <= c.PolicyEffDate)


UPDATE #TempVehCov 
	SET AgentCommissionRate = ISNULL(a.AgentCommissionRate,0), AgentCommissionAmt = ISNULL(round(c.WrittenPremiumAmt * a.AgentCommissionRate,2),0)
from  #TempVehCov  c
	INNER JOIN ICEBox..Policy p
		on c.PolicyNo = p.PolicyNo
		AND c.PolicyEffDate = p.PolicyEffDate
		AND c.PolicyTransactionNo = p.PolicyTransactionNo
		AND c.SourceSystemCode = p.SourceSystemCode
	INNER JOIN ICEBox..pgmAgentCommission a
		on p.ProgramCode = a.ProgramCode
		AND a.AgentCode = p.AgentCode
		and a.CommissionEffDate <= c.PolicyEffDate
		and a.CommissionEffDate = (select MAX(x.CommissionEffDate)
			FROM ICEBox..pgmAgentCommission x
				where x.ProgramCode = a.ProgramCode
					AND a.AgentCode = x.AgentCode
					AND x.CommissionEffDate <= c.PolicyEffDate)

--
-- ATTENTION 04-14-09 STILL NEED TO RE-CALC SegPrem when get correct dates.
Insert into VehicleCoverage
	select 
	a.PolicyNo as PolicyNo
	, a.PolicyEffdate as PolicyEffDate
	, a.VehicleNo as VehicleNo
	, a.PolicyTransactionNo as PolicyTransactionNo
	, a.CovCode as CovCode
	, a.SegEffDate as SegEffDate
	, a.SegExpDate as SegExpDate
	, a.InactivatedBy as InactivatedBy
	, a.InternalSublineCode as InternalSublineCode
	, a.WrittenPremiumAmt as WrittenPremiumAmt
	, a.TermPremiumAmt as TermPremiumAmt
	, a.SegWrittenPremiumAmt as SegWrittenPremiumAmt
	, a.AccountingPeriodNo as AccountingPeriodNo
	, a.SublineCode as SublineCode
	, a.RiskStateCode as RiskStateCode
	, a.FullyEarnedFlag as FullyEarnedFlag
	, a.ImmediateEarnedFlag as ImmediateEarnedFlag
	, a.SubjectToReinsuranceFlag as SubjectToReinsuranceFlag
	, a.AgentCommissionRate as AgentCommissionRate
	, a.AgentCommissionAmt as AgentCommissionAmt
	, NULL AS FrontingCommissionAmt
	, a.SourceInsertUserId as SourceInsertUserId
	, a.SourceInsertDT as SourceInsertDT
	, a.SourceSystemCode as SourceSystemCode
	, GETDATE() AS InsertDT 
from #TempVehCov A
where NOT EXISTS(SELECT * FROM VehicleCoverage v 
		where A.PolicyNo = v.PolicyNo
		and A.PolicyEffDate = v.PolicyEffDate
		and A.PolicyTransactionNo = v.PolicyTransactionNo
		and A.VehicleNo = v.VehicleNo
		and A.CovCode = v.CovCode 
		and A.SegEffDate = v.SegEffDate
		and A.SegExpDate = v.SegExpDate)



Insert into VehicleCoverage
	select 
	a.PolicyNo as PolicyNo
	, a.PolicyEffdate as PolicyEffDate
	, a.VehicleNo as VehicleNo
	, a.PolicyTransactionNo as PolicyTransactionNo
	, a.CovCode as CovCode
	, a.SegEffDate as SegEffDate
	, a.SegExpDate as SegExpDate
	, a.InactivatedBy as InactivatedBy
	, a.InternalSublineCode as InternalSublineCode
	, a.WrittenPremiumAmt as WrittenPremiumAmt
	, a.TermPremiumAmt as TermPremiumAmt
	, a.SegWrittenPremiumAmt as SegWrittenPremiumAmt
	, a.AccountingPeriodNo as AccountingPeriodNo
	, a.SublineCode as SublineCode
	, a.RiskStateCode as RiskStateCode
--	, a.TerritoryCode as TerritoryCode
	, a.FullyEarnedFlag as FullyEarnedFlag
	, a.ImmediateEarnedFlag as ImmediateEarnedFlag
	, a.SubjectToReinsuranceFlag as SubjectToReinsuranceFlag
	, a.AgentCommissionRate as AgentCommissionRate
	, a.AgentCommissionAmt as AgentCommissionAmt
	, NULL AS FrontingCommissionAmt
	, a.SourceInsertUserId as SourceInsertUserId
	, a.SourceInsertDT as SourceInsertDT
	, a.SourceSystemCode as SourceSystemCode
	, GETDATE() AS InsertDT 
from #IceBoxVCovSeg A 
where NOT EXISTS(SELECT * FROM VehicleCoverage v 
		where A.PolicyNo = v.PolicyNo
		and A.PolicyEffDate = v.PolicyEffDate
		and A.PolicyTransactionNo = v.PolicyTransactionNo
		and A.VehicleNo = v.VehicleNo
		and A.CovCode = v.CovCode 
		and A.SegEffDate = v.SegEffDate
		and A.SegExpDate = v.SegExpDate)


--WHERE PolicyNo = 'IFT800283-9'
--where cast(SUBSTRING(PolicyNO, 4, 6) as numeric) between 800209 and 800303
-- select * from VehicleCoverage where SourceInsertUserId = 'IFAP'
-- select * from #IceBoxVCovSeg where PolicyNo = 'IFT800283-9'
-- select * from #IceBoxVCovSeg order by PolicyNo, PolicyTransactionNo, VehicleNo, CovCode
-- select distinct PolicyNo,PolicyTransactionNo from #IceBoxVCovSeg order by PolicyNo, PolicyTransactionNo
/*
insert into VehicleCoverage from #IceBoxVCovSeg where PolicyNo = 'IFT800006-6'

*/

-- SELECT * FROM #IceBoxSeq

-- NEED TO Add SEGPREM & Segdates TO 'E' rec and to seg rec which should be in #IceBoxVCovSeg
-- delete from #IceBoxVCovSeg
-- select * from VehicleCoverage where SourceInsertUserId = 'IFAP' order by PolicyNo, PolicyEffDate, PolicyTransactionNo, VehicleNo




------------------------------------------------------------------------------------------------------------------------

GO
