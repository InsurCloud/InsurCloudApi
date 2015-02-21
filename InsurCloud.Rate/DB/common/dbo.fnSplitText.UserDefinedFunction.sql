USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[fnSplitText]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[fnSplitText]
(
      @List nvarchar(4000),
      @SplitOn nvarchar(5),
      @ReturnValue int
)  

RETURNS nvarchar(4000)
AS  
      BEGIN
            DECLARE @Counter int 
            SET @Counter = 0 
		SET @List = replace((replace(@List, '~~', '|')), '''', '')
            WHILE (Charindex(@SplitOn,@List)>0 and @Counter < (@ReturnValue - 1))
                  BEGIN 
                        SET @List = Substring(@List,Charindex(@SplitOn,@List)+len(@SplitOn),len(@List))
                        SET @Counter = @Counter + 1
                  END  
                  IF (@ReturnValue - 1) > @Counter
                        BEGIN 
                              SELECT @List = ''
                        END 
                  IF CHARINDEX(@SplitOn,@List)> 0
                        BEGIN 
                              SELECT @List = SUBSTRING(@List, 0,CHARINDEX(@SplitOn,@List))
                        END
      --          SELECT @List = @List + '-' + Convert(varchar(100),@Counter) + '-' + Convert(varchar(100),@ReturnValue)
                  RETURN @List
      END

GO
