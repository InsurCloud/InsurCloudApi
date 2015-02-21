USE [Common]
GO
/****** Object:  StoredProcedure [dbo].[PrintInfo2_Discrepancies]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrintInfo2_Discrepancies]
	
AS
BEGIN

	SET NOCOUNT ON;
	
	--117
	SELECT 'pgm117','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm117.dbo.printinfo2 t
		   JOIN pgm117.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm117','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm117.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm117.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union ALL
	--142
	SELECT 'pgm142','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm142.dbo.printinfo2 t
		   JOIN pgm142.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm142','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm142.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm142.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union all
	SELECT 'pgm202','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm202.dbo.printinfo2 t
		   JOIN pgm202.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm202','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm202.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm202.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union ALL
	SELECT 'pgm203','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm203.dbo.printinfo2 t
		   JOIN pgm203.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm203','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm203.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm203.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union ALL
	SELECT 'pgm209','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm209.dbo.printinfo2 t
		   JOIN pgm209.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm209','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm209.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm209.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union ALL
	SELECT 'pgm217','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm217.dbo.printinfo2 t
		   JOIN pgm217.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm217','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm217.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm217.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union ALL
	SELECT 'pgm235','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm235.dbo.printinfo2 t
		   JOIN pgm235.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm235','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm235.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm235.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	union ALL
	SELECT 'pgm242','Potential Overlap',t.PrintGroup,t.PrintCode,t.PrintSubCode
	FROM   pgm242.dbo.printinfo2 t
		   JOIN pgm242.dbo.printinfo2 p
			 ON t.Program = p.Program
				AND t.PrintGroup = p.PrintGroup
				AND t.PrintCode = p.PrintCode
				AND t.PrintSubCode = p.PrintSubCode
				AND t.EffDate > p.ExpDate
				AND t.ExpDate < p.ExpDate
	UNION ALL
	SELECT 'pgm242','Potential missing new record',old.PrintGroup,old.PrintCode,old.PrintSubCode
	FROM   pgm242.dbo.printinfo2 old
		   LEFT OUTER JOIN pgm242.dbo.printinfo2 new
			 ON old.Program = new.Program
				AND old.PrintGroup = new.PrintGroup
				AND old.PrintCode = new.PrintCode
				AND old.PrintSubCode = new.PrintSubCode
				AND old.ExpDate < new.ExpDate
	WHERE  old.ExpDate > Dateadd(d, -30, Getdate())
		   AND new.ExpDate IS NULL
		   AND old.ExpDate < Getdate() 
	
END

GO
