USE [Common]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_Split_Up_Ids]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






create FUNCTION [dbo].[fn_Split_Up_Ids]
(
   @Param_Ids varchar(7000)
)
RETURNS @Id_Table TABLE(IDField varchar(30))

AS
BEGIN  
   IF (LEN(@Param_Ids) <= 0) 
      RETURN

    DECLARE @CommaPos smallint
   SET @CommaPos = CHARINDEX(',', RTRIM(LTRIM(@Param_Ids)))	

   IF @CommaPos = 0
       INSERT INTO @Id_Table 
              VALUES(RTRIM(LTRIM(@Param_Ids)))
   ELSE 
       BEGIN
           WHILE LEN(@Param_Ids) > 1
	   BEGIN
	    SET @CommaPos = CHARINDEX(',', RTRIM(LTRIM(@Param_Ids)))
             INSERT INTO @Id_Table 
                      VALUES(SUBSTRING(RTRIM(LTRIM(@Param_Ids)),1, @CommaPos - 1))
	     SET @Param_Ids = SUBSTRING(RTRIM(LTRIM(@Param_Ids)), @CommaPos + 1 , LEN(RTRIM(LTRIM(@Param_Ids))))
	     SET @CommaPos = CHARINDEX(',', RTRIM(LTRIM(@Param_Ids)))
	     IF @CommaPos = 0
	     BEGIN
                 INSERT INTO @Id_Table VALUES(RTRIM(LTRIM(@Param_Ids)))
                 BREAK
	     END
	   END
       END
       RETURN 
  END




GO
