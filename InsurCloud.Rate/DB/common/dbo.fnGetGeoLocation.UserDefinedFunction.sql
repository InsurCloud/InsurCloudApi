USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[fnGetGeoLocation]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[fnGetGeoLocation]
(
	-- Add the parameters for the function here
	@IP varchar(20)
)
RETURNS varchar(50)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Country as varchar(50)

	declare @first as varchar(3)
declare @second as varchar(3)
declare @third as varchar(3)
declare @fourth as varchar(3)

SELECT
        @first=PARSENAME(@IP, 4),
        @second=PARSENAME(@IP, 3) ,
        @third=PARSENAME(@IP, 2) ,
        @fourth=PARSENAME(@IP, 1)
    
    
--SELECT @Country=Country
--FROM   Common..IPGeolocation
--WHERE   cast(@first as int) BETWEEN cast(startseg1 as int) AND cast(endseg1 as int)
--       AND cast(@second as int) BETWEEN cast(startseg2 as int) AND cast(endseg2 as int)
--       AND cast(@third as int) BETWEEN cast(startseg3 as int) AND cast(endseg3 as int)
--       AND cast(@fourth as int) BETWEEN cast(startseg4 as int) AND cast(endseg4 as int)
        
--NEED to Account for IPStart having larger # than IPEnd

--SELECT 
--	@IP = CASE 
--		WHEN CHARINDEX('.',@IP) < 4 
--			THEN STUFF(@IP,1,0,REPLICATE('0',4-CHARINDEX('.',@IP,1))) 
--		ELSE @IP 
--	END
--, 
--	@IP = CASE 
--		WHEN CHARINDEX('.',@IP) < 8 
--			THEN STUFF(@IP,5,0,REPLICATE('0',8-CHARINDEX('.',@IP, 5))) 
--		ELSE @IP 
--	END
--, 
--	@IP = CASE 
--		WHEN CHARINDEX('.',@IP) < 12 
--			THEN STUFF(@IP,9,0,REPLICATE('0',12-CHARINDEX('.',@IP, 9))) 
--		ELSE @IP 
--	END
--, 
--	@IP = CASE 
--		WHEN LEN(@IP) < 15 
--			THEN STUFF(@IP,13,0,REPLICATE('0',15-LEN(@IP))) 
--		ELSE @IP 
--	END

--SELECT @IP
        
--CAST(REPLACE(IP4,'.','') AS INT)
        
SELECT @Country=Country
FROM   Common..IPGeolocation
WHERE   
256 * 256 * 256 * CAST(PARSENAME(@IP, 4) AS float) + 256 * 256 * CAST(PARSENAME(@IP, 3) AS float) + 256 * CAST(PARSENAME(@IP, 2) AS float) + CAST(PARSENAME(@IP, 1) AS float)
between
256 * 256 * 256 * CAST(PARSENAME(IPStart, 4) AS float) + 256 * 256 * CAST(PARSENAME(IPStart, 3) AS float) + 256 * CAST(PARSENAME(IPStart, 2) AS float) + CAST(PARSENAME(IPStart, 1) AS float)
and
256 * 256 * 256 * CAST(PARSENAME(IPEnd, 4) AS float) + 256 * 256 * CAST(PARSENAME(IPEnd, 3) AS float) + 256 * CAST(PARSENAME(IPEnd, 2) AS float) + CAST(PARSENAME(IPEnd, 1) AS float)
--cast(@first as int) BETWEEN cast(Parsename(IPStart, 4) as int) AND cast(Parsename(IPEnd, 4) as int)
--       AND cast(@second as int) BETWEEN cast(Parsename(IPStart, 3) as int) AND cast(Parsename(IPEnd, 3) as int)
--       AND cast(@third as int) BETWEEN cast(Parsename(IPStart, 2) as int) AND cast(Parsename(IPEnd, 2) as int)
--       AND cast(@fourth as int) BETWEEN cast(Parsename(IPStart, 1) as int) AND cast(Parsename(IPEnd, 1) as int)

	-- Return the result of the function
	RETURN @Country

END

GO
