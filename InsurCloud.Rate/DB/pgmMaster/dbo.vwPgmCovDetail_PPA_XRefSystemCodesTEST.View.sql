USE [pgmMaster]
GO
/****** Object:  View [dbo].[vwPgmCovDetail_PPA_XRefSystemCodesTEST]    Script Date: 7/27/2014 4:24:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vwPgmCovDetail_PPA_XRefSystemCodesTEST]
AS
SELECT 
 CovCode
,CovGroup
,CovName
,UnitLimit
,LineLimit
,Deductible1
,Deductible2
,LowerRange
,UpperRange
,ShortDesc
,Desc1
,Desc2
,PolicyLevel
,SourceSystemCode
,SourceCovCode
FROM [DALSQLCS01\SQL01].PgmMaster.dbo.vwPgmCovDetail_PPA_XRefSystemCodes_TEST

GO
