USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[UpdateWeatherLookup]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[UpdateWeatherLookup]
	@Zip	NUMERIC(5,0),
	@xml	XML
AS

IF EXISTS(SELECT * FROM WeatherLookup WHERE ZipCode = @Zip)
	BEGIN

		UPDATE WeatherLookup
		SET LastLookupDate = GetDate(), LookupResult = @xml
		WHERE ZipCode = @Zip

	END
ELSE
	BEGIN
	
		INSERT INTO WeatherLookup (ZipCode, LastLookupDate, LookupResult)
		VALUES (@Zip, GetDate(), @xml)

	END

GO
