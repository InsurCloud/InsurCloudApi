USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[DriversLicRestrictionExistance]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROC [dbo].[DriversLicRestrictionExistance](
	@StateCode as Varchar(2), 
	@DLNo as VARCHAR(50),
	@UserCode as VARCHAR(50)
)
AS
BEGIN
	SET NOCOUNT ON
	/*-----------------------*/   
	-- declare temporary memory tables
	/*-----------------------*/   
	DECLARE @OUTPUT AS		TABLE(
		[Does Exist]		BIT,
		[Formatted Lic]		VARCHAR(100)	
	)
	IF (SELECT COUNT(*) FROM DLRestriction
		WHERE StateCode = @StateCode
		AND DLN = @DLNo) > 0
	BEGIN
		INSERT INTO @OUTPUT
		SELECT 
			1,
			LTRIM(RTRIM(StateCode)) + '-' + LTRIM(RTRIM(DLN))
		FROM DLRestriction
		WHERE StateCode = @StateCode
		AND DLN = @DLNo
	END
	ELSE
	BEGIN
		INSERT INTO DLRestriction
		SELECT @DLNo, @StateCode, @UserCode, GETDATE()
		INSERT INTO @OUTPUT
		SELECT 
			0,
			LTRIM(RTRIM(StateCode)) + '-' + LTRIM(RTRIM(DLN))
		FROM DLRestriction
		WHERE StateCode = @StateCode
		AND DLN = @DLNo
	END

	SELECT * FROM @OUTPUT

END

GO
