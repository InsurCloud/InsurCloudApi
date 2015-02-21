USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[RemoveDriversLicRestriction]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROC [dbo].[RemoveDriversLicRestriction](
	@StateCode as Varchar(2), 
	@DLNo as VARCHAR(50)
)
AS
BEGIN
	SET NOCOUNT ON
	/*-----------------------*/   
	-- declare temporary memory tables
	/*-----------------------*/   
	DECLARE @OUTPUT AS		TABLE(
		[Restriction Removed]		BIT
	)
	IF (SELECT COUNT(*) FROM DLRestriction
		WHERE StateCode = @StateCode
		AND DLN = @DLNo) > 0
	BEGIN
		DELETE FROM DLRestriction
		WHERE StateCode = @StateCode
		AND DLN = @DLNo
		INSERT INTO @OUTPUT
		SELECT 1
	END
	ELSE
	BEGIN
		INSERT INTO @OUTPUT
		SELECT 1		
	END

	SELECT * FROM @OUTPUT

END

GO
