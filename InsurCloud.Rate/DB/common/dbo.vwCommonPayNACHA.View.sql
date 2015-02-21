USE [Common]
GO
/****** Object:  View [dbo].[vwCommonPayNACHA]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vwCommonPayNACHA]
AS
SELECT     TOP (100) PERCENT PaymentID, PaymentMethodXML.value('(//CompanyCode)[1]', 'varchar(3)') AS CompanyCode, 
                      PaymentMethodXML.value('(//ProgramCode)[1]', 'varchar(4)') AS ProgramCode, PolicyNbr, PaymentAmt, PaymentDate, PostedDate, SweepDate, 
                      ReprocessedDate, PaymentMethodXML.value('(//AgentOfRecord)[1]', 'varchar(20)') AS AgentOfRecord, 
                      PaymentMethodXML.value('(//ReceiptingAgent)[1]', 'varchar(20)') AS AgentCode, PaymentMethodXML.value('(//InsuredName)[1]', 'varchar(100)') 
                      AS InsuredName, PaymentMethod, PaymentMethodXML, RTRIM(PaymentMethodXML.value('(//ReceiptingAgent)[1]', 'varchar(20)')) AS ReceiptAgent, 
                      RTRIM(PaymentMethodXML.value('(//AgentOfRecord)[1]', 'varchar(20)')) AS Expr1, 
                      UPPER(SUBSTRING(RTRIM(PaymentMethodXML.value('(//ReceiptingAgent)[1]', 'varchar(20)')), 1, 2)) AS Left2ReceiptCode, 
                      CASE WHEN RTrim(pay.PaymentMethodXML.value('(//ReceiptingAgent)[1]', 'varchar(20)')) 
                      = '' THEN RTrim(pay.PaymentMethodXML.value('(//AgentOfRecord)[1]', 'varchar(20)')) 
                      WHEN Upper(SubString(RTrim(pay.PaymentMethodXML.value('(//ReceiptingAgent)[1]', 'varchar(20)')), 1, 2)) 
                      = 'IF' THEN RTrim(pay.PaymentMethodXML.value('(//AgentOfRecord)[1]', 'varchar(20)')) 
                      ELSE RTrim(pay.PaymentMethodXML.value('(//ReceiptingAgent)[1]', 'varchar(20)')) END AS ReceiptingAgent
FROM         dbo.Payment AS Pay WITH (nolock)
WHERE     (CancelledDate IS NULL)
ORDER BY PaymentAmt DESC

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
         Begin Table = "Pay"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 143
               Right = 248
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwCommonPayNACHA'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vwCommonPayNACHA'
GO
