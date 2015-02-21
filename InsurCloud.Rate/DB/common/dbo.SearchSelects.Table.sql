USE [Common]
GO
/****** Object:  Table [dbo].[SearchSelects]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SearchSelects](
	[SearchType] [varchar](50) NOT NULL,
	[ColumnName] [varchar](50) NOT NULL,
	[DisplayDescription] [varchar](50) NOT NULL,
	[DisplayOrder] [numeric](18, 0) NOT NULL,
	[JoinID] [numeric](18, 0) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'PolicyNo', N'Policy ID', CAST(1 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'PolicyStatusInd', N'Policy Status Ind', CAST(2 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'TermEffDate', N'Term Effective Date', CAST(3 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'PolicyTransactionNum', N'Transaction', CAST(4 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'PolicyExpDate', N'Expiration Date', CAST(5 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'AgentCode', N'Agent Of Record', CAST(6 AS Numeric(18, 0)), CAST(0 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'FirstName', N'Insured`s First Name', CAST(7 AS Numeric(18, 0)), CAST(1 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
INSERT [dbo].[SearchSelects] ([SearchType], [ColumnName], [DisplayDescription], [DisplayOrder], [JoinID], [EffDate], [ExpDate]) VALUES (N'POLICY', N'LastName', N'Insured`s Last Name', CAST(8 AS Numeric(18, 0)), CAST(1 AS Numeric(18, 0)), CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime))
