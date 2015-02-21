USE [Common]
GO
/****** Object:  Table [dbo].[RenewalRules]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RenewalRules](
	[ID] [numeric](6, 0) IDENTITY(0,1) NOT NULL,
	[Product] [varchar](50) NOT NULL,
	[FunctionName] [varchar](50) NOT NULL,
	[ProcessLevel] [varchar](50) NOT NULL,
	[PolicyStatusFlag] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[OrderNum] [int] NOT NULL,
 CONSTRAINT [PK_RenewalRules] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET IDENTITY_INSERT [dbo].[RenewalRules] ON 

INSERT [dbo].[RenewalRules] ([ID], [Product], [FunctionName], [ProcessLevel], [PolicyStatusFlag], [EffDate], [ExpDate], [OrderNum]) VALUES (CAST(0 AS Numeric(6, 0)), N'2', N'OrderReports', N'POLICY', N'E', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 1)
INSERT [dbo].[RenewalRules] ([ID], [Product], [FunctionName], [ProcessLevel], [PolicyStatusFlag], [EffDate], [ExpDate], [OrderNum]) VALUES (CAST(1 AS Numeric(6, 0)), N'ALL', N'ValidateAgent', N'POLICY', N'P', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 2)
INSERT [dbo].[RenewalRules] ([ID], [Product], [FunctionName], [ProcessLevel], [PolicyStatusFlag], [EffDate], [ExpDate], [OrderNum]) VALUES (CAST(2 AS Numeric(6, 0)), N'1', N'ProcessInflationGuard', N'POLICY', N'P', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 3)
INSERT [dbo].[RenewalRules] ([ID], [Product], [FunctionName], [ProcessLevel], [PolicyStatusFlag], [EffDate], [ExpDate], [OrderNum]) VALUES (CAST(3 AS Numeric(6, 0)), N'1', N'UpdateLossOfUseAmt', N'POLICY', N'P', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 4)
INSERT [dbo].[RenewalRules] ([ID], [Product], [FunctionName], [ProcessLevel], [PolicyStatusFlag], [EffDate], [ExpDate], [OrderNum]) VALUES (CAST(4 AS Numeric(6, 0)), N'1', N'UpdatePriorInsuranceInfo', N'POLICY', N'P', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 5)
INSERT [dbo].[RenewalRules] ([ID], [Product], [FunctionName], [ProcessLevel], [PolicyStatusFlag], [EffDate], [ExpDate], [OrderNum]) VALUES (CAST(5 AS Numeric(6, 0)), N'1', N'OrderReports', N'POLICY', N'P', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 6)
SET IDENTITY_INSERT [dbo].[RenewalRules] OFF
