USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[fnSplit]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--Author : Himanshu Patel
--Date Created : 05/01/2007
--Purpose - function is same as C#, only that it returns table not array.
CREATE FUNCTION [dbo].[fnSplit]
(
	@Delimiter CHAR,
	@Text VARCHAR(8000)
)
RETURNS @Result TABLE (RowID SMALLINT IDENTITY(1, 1) PRIMARY KEY, Data VARCHAR(8000))
AS

BEGIN
	DECLARE	@NextPos SMALLINT,
		@LastPos SMALLINT

	SELECT	@NextPos = 0

	WHILE @NextPos <= DATALENGTH(@Text)
		BEGIN
			SELECT	@LastPos = @NextPos,
				@NextPos =	CASE
							WHEN CHARINDEX(@Delimiter, @Text, @LastPos + 1) = 0 THEN DATALENGTH(@Text) + 1
							ELSE CHARINDEX(@Delimiter, @Text, @LastPos + 1)
						END

			INSERT	@Result
				(
					Data
				)
			SELECT	SUBSTRING(@Text, @LastPos + 1, @NextPos - @LastPos - 1)
		END
		
	RETURN
END


GO
