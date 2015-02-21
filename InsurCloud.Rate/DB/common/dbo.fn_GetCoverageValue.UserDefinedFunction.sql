USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_GetCoverageValue]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


Create Function [dbo].[fn_GetCoverageValue] (
	@prmField nVarChar(15),
	@prmSide varChar(3) = 'Min')
Returns Int
With Execute as Caller
As
	Begin
/* ****************************************************************************************
 *	fn_GetCoverageValue (
 *		@prmField nVarChar(15) - the description field where a value needs to be returned,
 *		@prmSide varChar(3) - defaults to Min for the side to return
 *		-- Only acceptable values for prmSide will be 'MIN' and 'MAX'
 *******************************************************************************************
*/
		Declare @pos int; -- @pos tries to identify the position of the delimiter
		Declare @convertVal varchar(15); -- @convertVal grabs the side of the passed field value
		Declare @returnVal Int; -- @returnVal is the value that gets returned by the function 
		
		Set @pos = 0;
		Set @convertVal = '';
		Set @returnVal = 0;
		-- Find the delimiter location		
		Set @pos = (patindex('%[/-]%', @prmField) );

		If (@pos > 0)
			Begin
				If (Upper(@prmSide) = 'MIN')
					Set @convertVal = Substring(@prmField, 0, @pos);
				Else 
					If (Upper(@prmSide) = 'MAX')
						Set @convertVal = Substring(@prmField, @pos + 1, Len(@prmField));
			End
		Else -- Find if there are any other non-numeric characters
			Begin
			Set @pos = (patindex('%[^0-9]%', @prmField) );
				If (@pos > 1) -- Text would start at position 0
					Set @convertVal = Substring(@prmField, 0, @pos);
				Else
					If (IsNumeric(@prmField)=1) -- This is just a value, so it needs to get converted
						Set @convertVal = @prmField;
			End

		If (IsNumeric(@convertVal)= 1)
			Set @returnVal = Abs(Cast(@convertVal as Int));
		Else
			Begin
				If (Upper(@prmSide) = 'MIN')
					Set @returnVal = -1;
				
				If (Upper(@prmSide) = 'MAX')
					Set @returnVal = 9999999;
			End

		Return @returnVal;
	End

GO
