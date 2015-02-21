USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[udfGenerateSequence]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE FUNCTION [dbo].[udfGenerateSequence](@sUserID [nvarchar](50), @sSequenceName [nvarchar](50))
RETURNS [nvarchar](256) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [GenerateSequence].[UserDefinedFunctions].[udfGenerateSequence]
GO
