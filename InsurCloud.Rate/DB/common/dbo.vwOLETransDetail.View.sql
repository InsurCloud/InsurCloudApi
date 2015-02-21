USE [Common]
GO
/****** Object:  View [dbo].[vwOLETransDetail]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vwOLETransDetail]
AS
Select ed.PolicyID, ed.EndXML.value('(clsEndorsementManager/OriginalPolicy/OrigTermEffDate)[1]', 'datetime') as TermEffDate,
	ed.TransNbr, ed.ChangeEffDate, ed.AgentId, ed.Program, ed.UploadTS,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[1]/@xsi:type', 'varchar(50)'), 4, 50) as Type1,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[2]/@xsi:type', 'varchar(50)'), 4, 50) as Type2,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[3]/@xsi:type', 'varchar(50)'), 4, 50) as Type3,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[4]/@xsi:type', 'varchar(50)'), 4, 50) as Type4,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[5]/@xsi:type', 'varchar(50)'), 4, 50) as Type5,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[6]/@xsi:type', 'varchar(50)'), 4, 50) as Type6,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[7]/@xsi:type', 'varchar(50)'), 4, 50) as Type7,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[8]/@xsi:type', 'varchar(50)'), 4, 50) as Type8,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[9]/@xsi:type', 'varchar(50)'), 4, 50) as Type9,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[10]/@xsi:type', 'varchar(50)'), 4, 50) as Type10
From pgm235..EndorsementXML ed
Inner Join
(
Select Max(ID) as ID, PolicyID, ChangeEffDate, TransNbr From pgm235..EndorsementXML
Where Status = 'COMMIT'
Group By PolicyID, ChangeEffDate, TransNbr
) as A on ed.ID = A.ID

UNION ALL

Select ed.PolicyID, ed.EndXML.value('(clsEndorsementManager/OriginalPolicy/OrigTermEffDate)[1]', 'datetime') as TermEffDate,
	ed.TransNbr, ed.ChangeEffDate, ed.AgentId, ed.Program, ed.UploadTS,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[1]/@xsi:type', 'varchar(50)'), 4, 50) as Type1,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[2]/@xsi:type', 'varchar(50)'), 4, 50) as Type2,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[3]/@xsi:type', 'varchar(50)'), 4, 50) as Type3,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[4]/@xsi:type', 'varchar(50)'), 4, 50) as Type4,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[5]/@xsi:type', 'varchar(50)'), 4, 50) as Type5,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[6]/@xsi:type', 'varchar(50)'), 4, 50) as Type6,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[7]/@xsi:type', 'varchar(50)'), 4, 50) as Type7,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[8]/@xsi:type', 'varchar(50)'), 4, 50) as Type8,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[9]/@xsi:type', 'varchar(50)'), 4, 50) as Type9,
	substring(ed.EndXML.value('(clsEndorsementManager/EndorsementColl/clsBaseEndorsement)[10]/@xsi:type', 'varchar(50)'), 4, 50) as Type10
From pgm217..EndorsementXML ed
Inner Join
(
Select Max(ID) as ID, PolicyID, ChangeEffDate, TransNbr From pgm217..EndorsementXML
Where Status = 'COMMIT'
Group By PolicyID, ChangeEffDate, TransNbr
) as A on ed.ID = A.ID



GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwOLETransDetail'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwOLETransDetail'
GO
