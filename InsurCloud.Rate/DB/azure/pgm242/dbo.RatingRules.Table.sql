/****** Object:  Table [dbo].[RatingRules]    Script Date: 7/26/2014 4:43:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RatingRules](
	[ID] [int] NOT NULL,
	[CallingSystem] [varchar](50) NOT NULL,
	[RuleType] [varchar](50) NOT NULL,
	[State] [varchar](50) NOT NULL,
	[Program] [varchar](50) NOT NULL,
	[FunctionName] [varchar](50) NOT NULL,
	[SubType] [varchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[OrderNumber] [int] NOT NULL,
 CONSTRAINT [PK_RatingRules] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242002, N'ALL', N'IER', N'42', N'ALL', N'CheckDWICountUnder21', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242002)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242003, N'ALL', N'IER', N'42', N'ALL', N'CheckDWICount', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242003)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242004, N'ALL', N'IER', N'42', N'ALL', N'CheckArtistanUse', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242004)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242005, N'ALL', N'IER', N'42', N'ALL', N'CheckOutOfStateZip', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242005)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242006, N'ALL', N'UWW', N'42', N'ALL', N'CheckDriverPointsClassic', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242006)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242007, N'ALL', N'UWW', N'42', N'ALL', N'CheckDriverPointsClassic', N'DRIVER', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242007)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242008, N'ALL', N'UWW', N'42', N'ALL', N'CheckDriverViolationsClassic', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242008)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242009, N'ALL', N'UWW', N'42', N'ALL', N'CheckDriverViolationsClassic', N'DRIVER', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242009)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242010, N'ALL', N'IER', N'42', N'ALL', N'CheckTotalPoints', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242010)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242011, N'ALL', N'IER', N'42', N'ALL', N'CheckPolicyPoints', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242011)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242012, N'WEBRATER', N'UWW', N'42', N'ALL', N'CheckClaimActivity', N'POLICY', 3, CAST(0x0000A07700000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242012)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242013, N'ALL', N'IER', N'42', N'DIRECT', N'CheckPhysicalDamageSymbols2010AndOlder', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000A28700000000 AS DateTime), 242013)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242014, N'ALL', N'IER', N'42', N'ALL', N'CheckPhysicalDamageSymbols2011AndNewer', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000A28700000000 AS DateTime), 242014)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242015, N'ALL', N'IER', N'42', N'DIRECT', N'CheckVINLength', N'POLICY', 3, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 242015)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242016, N'ALL', N'IER', N'42', N'ALL', N'CheckMinimumPermitAge', N'POLICY', 1, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 242016)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242017, N'ALL', N'IER', N'42', N'DIRECT', N'CheckGaragingZip', N'POLICY', 2, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 242017)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242018, N'ALL', N'IER', N'42', N'DIRECT', N'CheckMarried', N'POLICY', 2, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 242018)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242019, N'ALL', N'IER', N'42', N'DIRECT', N'CheckDLDupes', N'POLICY', 2, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 242019)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242020, N'ALL', N'IER', N'42', N'DIRECT', N'CheckUnlistedAdditionalDrivers', N'POLICY', 3, CAST(0x00009A1600000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 242020)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242021, N'ALL', N'IER', N'42', N'CLASSIC', N'CheckMinimumAge', N'POLICY', 1, CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242021)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242022, N'ALL', N'IER', N'42', N'DIRECT', N'CheckMinimumAge', N'POLICY', 3, CAST(0x0000A15800000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242022)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242031, N'ALL', N'IER', N'42', N'ALL', N'CheckSalvagedPhysicalDamage', N'POLICY', 3, CAST(0x00009EAA00000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242031)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242033, N'ALL', N'IER', N'42', N'ALL', N'CheckVehicleAge', N'POLICY', 1, CAST(0x00009EAA00000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242033)
INSERT [dbo].[RatingRules] ([ID], [CallingSystem], [RuleType], [State], [Program], [FunctionName], [SubType], [Status], [EffDate], [ExpDate], [OrderNumber]) VALUES (242035, N'ALL', N'IER', N'42', N'ALL', N'CheckSymbol2', N'POLICY', 1, CAST(0x00009EAA00000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime), 242035)
