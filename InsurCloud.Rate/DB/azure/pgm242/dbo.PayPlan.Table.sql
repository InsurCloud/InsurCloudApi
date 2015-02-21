/****** Object:  Table [dbo].[PayPlan]    Script Date: 7/26/2014 4:43:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PayPlan](
	[Program] [varchar](10) NOT NULL,
	[PayPlanCode] [char](3) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[DownPayPct] [numeric](18, 2) NOT NULL,
	[NumInstallments] [numeric](18, 0) NOT NULL,
	[InstallmentType] [varchar](50) NOT NULL,
	[InstallmentFeeCode] [varchar](9) NULL,
	[EFTInstallmentFeeCode] [varchar](9) NULL,
	[UsePremWFeesInCalc] [bit] NOT NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
	[FirstInstallmentType] [varchar](50) NULL,
	[FirstInstallmentInterval] [numeric](18, 0) NULL,
	[InstallmentPct] [numeric](18, 2) NULL,
	[InstallmentInterval] [numeric](18, 7) NULL,
	[InvoiceToDueDate] [numeric](18, 0) NULL,
	[InstallmentFeeOnDownPayYN] [bit] NULL,
 CONSTRAINT [PK_PayPlan] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[PayPlanCode] ASC,
	[AppliesToCode] ASC,
	[EffDate] ASC,
	[ExpDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'100', N'Full Pay', CAST(100.00 AS Numeric(18, 2)), CAST(0 AS Numeric(18, 0)), N'ANNUAL', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B6500000000 AS DateTime), CAST(0x00009BBF00000000 AS DateTime), N'dustin.dorris', CAST(0x00009C18009AC9C8 AS DateTime), N'ANNUAL', CAST(0 AS Numeric(18, 0)), CAST(0.00 AS Numeric(18, 2)), CAST(0.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'100', N'Full Pay', CAST(100.00 AS Numeric(18, 2)), CAST(0 AS Numeric(18, 0)), N'ANNUAL', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'ANNUAL', CAST(0 AS Numeric(18, 0)), CAST(0.00 AS Numeric(18, 2)), CAST(0.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'100', N'Full Pay', CAST(100.00 AS Numeric(18, 2)), CAST(0 AS Numeric(18, 0)), N'ANNUAL', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'ANNUAL', CAST(0 AS Numeric(18, 0)), CAST(0.00 AS Numeric(18, 2)), CAST(0.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'205', N'20% Down, 5 Installments', CAST(20.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B6500000000 AS DateTime), CAST(0x00009BBF00000000 AS DateTime), N'dustin.dorris', CAST(0x00009C18009AC9C8 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'205', N'20% Down, 5 Installments', CAST(20.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'205', N'20% Down, 5 Installments', CAST(20.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'terry.johnson', CAST(0x0000A1BE017AC389 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'215', N'21% Down, 5 Installments', CAST(21.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B6500000000 AS DateTime), CAST(0x00009BBF00000000 AS DateTime), N'dustin.dorris', CAST(0x00009C18009AC9C8 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.80 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'215', N'21% Down, 5 Installments', CAST(21.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009C3B00000000 AS DateTime), N'kevin.bowser', CAST(0x00009C3A010C3CB0 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.80 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'215', N'21% Down, 5 Installments', CAST(21.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x00009F2B00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.80 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'254', N'25% Down 4 Payments', CAST(25.00 AS Numeric(18, 2)), CAST(4 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B6500000000 AS DateTime), CAST(0x00009BBF00000000 AS DateTime), N'dustin.dorris', CAST(0x00009C18009AC9C8 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(18.75 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'254', N'25% Down 4 Payments', CAST(25.00 AS Numeric(18, 2)), CAST(4 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(18.75 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'254', N'25% Down 4 Payments', CAST(25.00 AS Numeric(18, 2)), CAST(4 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(18.75 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'255', N'25% Down, 5 Installments', CAST(25.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B6500000000 AS DateTime), CAST(0x00009BBF00000000 AS DateTime), N'dustin.dorris', CAST(0x00009C18009AC9C8 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'255', N'25% Down, 5 Installments', CAST(25.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'255', N'25% Down, 5 Installments', CAST(25.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'MTA', N'Even 6 Monthly', CAST(17.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B6500000000 AS DateTime), CAST(0x00009BBF00000000 AS DateTime), N'dustin.dorris', CAST(0x00009C18009AC9C8 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'MTA', N'Even 6 Monthly', CAST(17.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D6DC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'MTA', N'Even 6 Monthly', CAST(16.66 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'terry.johnson', CAST(0x0000A17E012D6573 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Classic', N'RPP', N'Renewal Pay Plan', CAST(16.70 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'R', CAST(0x00009BAC00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'mindy.arvisu', CAST(0x00009E0000BE6517 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 1)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Direct', N'100', N'Full Pay', CAST(100.00 AS Numeric(18, 2)), CAST(0 AS Numeric(18, 0)), N'ANNUAL', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A10200937B44 AS DateTime), N'ANNUAL', CAST(0 AS Numeric(18, 0)), CAST(0.00 AS Numeric(18, 2)), CAST(0.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Direct', N'205', N'20% Down, 5 Installments', CAST(20.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'terry.johnson', CAST(0x0000A1BE017AC389 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Direct', N'254', N'25% Down, 4 Installments', CAST(25.00 AS Numeric(18, 2)), CAST(4 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A102009625AC AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(18.75 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Direct', N'255', N'25% Down, 5 Installments', CAST(25.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A1020096AD52 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Direct', N'MTA', N'Even 6 Monthly', CAST(16.66 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'terry.johnson', CAST(0x0000A17E012D6573 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Direct', N'RPP', N'Renewal Pay Plan', CAST(16.70 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'R', CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'terry.johnson', CAST(0x0000A0FD00CD98D3 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 1)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'100', N'Full Pay', CAST(100.00 AS Numeric(18, 2)), CAST(0 AS Numeric(18, 0)), N'ANNUAL', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB5E50 AS DateTime), N'ANNUAL', CAST(0 AS Numeric(18, 0)), CAST(0.00 AS Numeric(18, 2)), CAST(0.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'205', N'20% Down, 5 Installments', CAST(20.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB5E50 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'215', N'21% Down, 5 Installments', CAST(21.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB5E50 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.80 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'254', N'25% Down 4 Payments', CAST(25.00 AS Numeric(18, 2)), CAST(4 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB5E50 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(18.75 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'255', N'25% Down, 5 Installments', CAST(25.00 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB5E50 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(15.00 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'MTA', N'Even 6 Monthly', CAST(16.66 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB5E50 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 0)
INSERT [dbo].[PayPlan] ([Program], [PayPlanCode], [Name], [DownPayPct], [NumInstallments], [InstallmentType], [InstallmentFeeCode], [EFTInstallmentFeeCode], [UsePremWFeesInCalc], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [FirstInstallmentType], [FirstInstallmentInterval], [InstallmentPct], [InstallmentInterval], [InvoiceToDueDate], [InstallmentFeeOnDownPayYN]) VALUES (N'Summit', N'RPP', N'Renewal Pay Plan', CAST(16.70 AS Numeric(18, 2)), CAST(5 AS Numeric(18, 0)), N'MONTHLY', N'INSTALL', N'EFTINSTAL', 1, N'R', CAST(0x00009BAC00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'mindy.arvisu', CAST(0x00009E0000BE6517 AS DateTime), N'MONTHLY', CAST(1 AS Numeric(18, 0)), CAST(16.66 AS Numeric(18, 2)), CAST(1.0000000 AS Numeric(18, 7)), CAST(15 AS Numeric(18, 0)), 1)
