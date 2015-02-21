USE [pgmMaster]
GO
/****** Object:  View [dbo].[vwPgmCovDetail_PPA_XRefSystemCodes]    Script Date: 7/27/2014 4:24:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vwPgmCovDetail_PPA_XRefSystemCodes]
AS
SELECT TOP 100 PERCENT
 PgmCovDetail_PPA.CovCode
,PgmCovDetail_PPA.CovGroup
,PgmCovDetail_PPA.CovName
,PgmCovDetail_PPA.UnitLimit
,PgmCovDetail_PPA.LineLimit
,PgmCovDetail_PPA.Deductible1
,PgmCovDetail_PPA.Deductible2
,PgmCovDetail_PPA.LowerRange
,PgmCovDetail_PPA.UpperRange
,PgmCovDetail_PPA.ShortDesc
,PgmCovDetail_PPA.Desc1
,PgmCovDetail_PPA.Desc2
,PgmCovDetail_PPA.PolicyLevel
,XRefSystemCodes.SourceSystemCode
,XRefSystemCodes.SourceCovCode
FROM dbo.PgmCovDetail_PPA
INNER JOIN dbo.XRefSystemCodes On PgmCovDetail_PPA.CovCode = XRefSystemCodes.CovCode
ORDER BY LowOrderDispl, HighOrderDispl

GO
