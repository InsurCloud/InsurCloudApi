/****** Object:  Table [dbo].[FactorFee]    Script Date: 7/26/2014 4:43:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[FactorFee](
	[Program] [varchar](10) NOT NULL,
	[FeeCode] [varchar](9) NOT NULL,
	[Description] [varchar](50) NOT NULL,
	[FeeApplicationType] [varchar](50) NOT NULL,
	[Factor] [decimal](9, 4) NOT NULL,
	[FactorType] [varchar](8) NOT NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
	[SubSystemCode] [varchar](3) NULL,
	[TransactionAccountCode] [varchar](3) NULL,
	[TransactionTypeCode] [varchar](3) NULL,
 CONSTRAINT [PK_FactorFee] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[FeeCode] ASC,
	[AppliesToCode] ASC,
	[EffDate] ASC,
	[ExpDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'EFTINSTAL', N'EFT Installment Fee', N'EARNED', CAST(2.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'EFTINSTAL', N'EFT Installment Fee', N'EARNED', CAST(2.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'INSTALL', N'Installment Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'INSTALL', N'Installment Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'LATE', N'Late Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'LATE', N'Late Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'NSF', N'NSF Fee', N'EARNED', CAST(25.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'NSF', N'NSF Fee', N'EARNED', CAST(25.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'POLICY', N'Policy Fee', N'SPREAD', CAST(66.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'POLICY', N'Policy Fee', N'SPREAD', CAST(66.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'RIFEE', N'Reinstatement Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'SR22', N'SR-22 Fee', N'EARNED', CAST(35.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009BBF00000000 AS DateTime), CAST(0x00009D6900000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'SR22', N'SR-22 Fee', N'EARNED', CAST(35.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D580088D22C AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'THEFT', N'TX Vehicle Theft Prevention Fee', N'EARNED', CAST(0.5000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x00009F5100000000 AS DateTime), N'kevin.bowser', CAST(0x00009DEB00FAA328 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Classic', N'THEFT', N'TX Vehicle Theft Prevention Fee', N'EARNED', CAST(1.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009F5100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'mindy.arvisu', CAST(0x00009F3D0105FC41 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'EFTINSTAL', N'EFT Installment Fee', N'EARNED', CAST(2.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A1010115EBD4 AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'INSTALL', N'Installment Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A10101160FE0 AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'LATE', N'Late Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A10101169FA9 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'NSF', N'NSF Fee', N'EARNED', CAST(25.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A1010116E242 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'POLICY', N'Policy Fee', N'SPREAD', CAST(66.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A10101166B65 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'RIFEE', N'Reinstatement Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A10101174F17 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'SR22', N'SR-22 Fee', N'EARNED', CAST(35.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A101011711E5 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Direct', N'THEFT', N'TX Vehicle Theft Prevention Fee', N'EARNED', CAST(1.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x0000A15700000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'maxwell.ochieng', CAST(0x0000A10101177170 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'EFTINSTAL', N'EFT Installment Fee', N'EARNED', CAST(2.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB4B90 AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'INSTALL', N'Installment Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB4B90 AS DateTime), N'A', N'IF', N'IF')
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'LATE', N'Late Fee', N'EARNED', CAST(6.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB4B90 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'NSF', N'NSF Fee', N'EARNED', CAST(25.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB4B90 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'POLICY', N'Policy Fee', N'SPREAD', CAST(50.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'mindy.arvisu', CAST(0x00009E4C00BF2125 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'SR22', N'SR-22 Fee', N'EARNED', CAST(25.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009B8400000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.bowser', CAST(0x00009D9E00AB4B90 AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'THEFT', N'TX Vehicle Theft Prevention Fee', N'EARNED', CAST(0.5000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009D6900000000 AS DateTime), CAST(0x00009F5100000000 AS DateTime), N'kevin.bowser', CAST(0x00009DEB00FAA32B AS DateTime), NULL, NULL, NULL)
INSERT [dbo].[FactorFee] ([Program], [FeeCode], [Description], [FeeApplicationType], [Factor], [FactorType], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS], [SubSystemCode], [TransactionAccountCode], [TransactionTypeCode]) VALUES (N'Summit', N'THEFT', N'TX Vehicle Theft Prevention Fee', N'EARNED', CAST(1.0000 AS Decimal(9, 4)), N'PostAdd', N'B', CAST(0x00009F5100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'mindy.arvisu', CAST(0x00009F3D0105FC41 AS DateTime), NULL, NULL, NULL)
