USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[InstallmentsNoBatchMsgRecon]    Script Date: 7/27/2014 2:06:56 PM ******/
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
CREATE PROCEDURE [dbo].[InstallmentsNoBatchMsgRecon]
      -- Add the parameters for the stored procedure here
      (@runDate datetime, @minBalance numeric (9,2))
AS

BEGIN

	--  FOR TESTING PURPOSES
	--Declare @runDate datetime, @minBalance numeric (9,2)
	--set @runDate='5/7/2012'
	--set @minBalance='5'
	--  END FOR TESTING PURPOSES
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '117','H20',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '117','H30',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '117','H3T',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '117','DP1',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '117','DP2',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '117','DP3',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '142','HOA',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '142','HOB',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '142','HOT',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '142','TDP',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '202','AZC',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '203','ARC',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '203','AR6',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '209','FLC',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '217','LAC',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '217','LA6',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '235','OKC',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '235','OK6',@runDate,@minBalance
	
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '242','TAA',@runDate,@minBalance
	exec Common.dbo.InstallmentsWithoutBatchMsgs2 '242','TX6',@runDate,@minBalance
	--select 'Done'
END 
SET QUOTED_IDENTIFIER OFF



GO
