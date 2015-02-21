USE [pgm242]
GO
/****** Object:  View [dbo].[PolicyXML_ReducedDuplicateInsureds]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[PolicyXML_ReducedDuplicateInsureds]
AS
SELECT     TOP (100) PERCENT pxml.QuoteID, pxml.PolicyID, pxml.Product, pxml.State, pxml.AgencyID, pxml.FirstName, pxml.LastName, pxml.SavedDate, pxml.CreditOrdered, 
                      pxml.CreditMsg, pxml.Premium, pxml.Fees, pxml.Status, pxml.PolicyXML, pxml.UploadTS, pxml.StartDate
FROM         (SELECT     AgencyID, FirstName, LastName, MAX(PolicyID) AS PolicyID, MAX(QuoteID) AS QuoteID
                       FROM          dbo.PolicyXML WITH (NOLOCK)
                       WHERE      (Premium IS NOT NULL)
                       GROUP BY AgencyID, LastName, FirstName) AS groupXML INNER JOIN
                      dbo.PolicyXML AS pxml WITH (NOLOCK) ON groupXML.FirstName = pxml.FirstName AND groupXML.LastName = pxml.LastName AND 
                      (groupXML.PolicyID IS NOT NULL AND groupXML.PolicyID = pxml.PolicyID OR
                      groupXML.PolicyID IS NULL AND groupXML.QuoteID = pxml.QuoteID)
WHERE     (pxml.Premium IS NOT NULL) AND (pxml.AgencyID NOT LIKE '%9999%') AND (pxml.AgencyID <> '20000') AND (pxml.AgencyID <> 'ifacadmin') AND 
                      (UPPER(pxml.Status) <> 'EXTERNAL')
ORDER BY groupXML.LastName, groupXML.FirstName


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
         Begin Table = "groupXML"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 239
               Right = 198
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "pxml"
            Begin Extent = 
               Top = 12
               Left = 414
               Bottom = 229
               Right = 574
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 19
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PolicyXML_ReducedDuplicateInsureds'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'PolicyXML_ReducedDuplicateInsureds'
GO
