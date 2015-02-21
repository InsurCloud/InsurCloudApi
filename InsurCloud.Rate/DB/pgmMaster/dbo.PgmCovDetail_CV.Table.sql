USE [pgmMaster]
GO
/****** Object:  Table [dbo].[PgmCovDetail_CV]    Script Date: 7/27/2014 4:25:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PgmCovDetail_CV](
	[CovCode] [numeric](18, 0) NOT NULL,
	[CovGroup] [nvarchar](50) NOT NULL,
	[CovName] [varchar](100) NOT NULL,
	[UnitLimit] [numeric](18, 0) NULL,
	[LineLimit] [numeric](18, 0) NULL,
	[Deductible] [numeric](18, 0) NULL,
	[Deductible2] [numeric](18, 0) NULL,
	[ShiftType] [varchar](5) NULL,
	[LowerRange] [numeric](18, 0) NULL,
	[UpperRange] [numeric](18, 0) NULL,
	[LowOrderDispl] [numeric](18, 0) NOT NULL,
	[HighOrderDispl] [numeric](18, 0) NOT NULL,
	[ShortDesc] [varchar](100) NOT NULL,
	[Desc1] [varchar](50) NULL,
	[Desc2] [varchar](50) NULL,
	[PolicyLevel] [bit] NOT NULL,
	[InsertDT] [smalldatetime] NOT NULL,
 CONSTRAINT [PK_CVPgmCovDetail_1] PRIMARY KEY CLUSTERED 
(
	[CovCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(4020010 AS Numeric(18, 0)), N'BI ', N'Bodily Injury Liability', CAST(10000 AS Numeric(18, 0)), CAST(20000 AS Numeric(18, 0)), NULL, NULL, N'S', NULL, NULL, CAST(2000 AS Numeric(18, 0)), CAST(10 AS Numeric(18, 0)), N'10/20', N'$10,000 each person', N'$20,000 each accident', 1, CAST(0x9CE1040F AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(4020020 AS Numeric(18, 0)), N'BI ', N'Bodily Injury Liability', CAST(15000 AS Numeric(18, 0)), CAST(30000 AS Numeric(18, 0)), NULL, NULL, N'S', NULL, NULL, CAST(2000 AS Numeric(18, 0)), CAST(20 AS Numeric(18, 0)), N'15/30', N'$15,000 each person', N'$30,000 each accident', 1, CAST(0x9CE1040F AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(4020050 AS Numeric(18, 0)), N'BI ', N'Bodily Injury Liability', CAST(25000 AS Numeric(18, 0)), CAST(50000 AS Numeric(18, 0)), NULL, NULL, N'S', NULL, NULL, CAST(1000 AS Numeric(18, 0)), CAST(10 AS Numeric(18, 0)), N'25/50', N'$25,000 each person', N'$50,000 each accident', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(4020051 AS Numeric(18, 0)), N'BI ', N'Bodily Injury Liability', CAST(25000 AS Numeric(18, 0)), CAST(50000 AS Numeric(18, 0)), NULL, NULL, N'D', NULL, NULL, CAST(1000 AS Numeric(18, 0)), CAST(20 AS Numeric(18, 0)), N'25/50', N'$25,000 each person', N'$50,000 each accident', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(4020070 AS Numeric(18, 0)), N'BI ', N'Bodily Injury Liability', CAST(50000 AS Numeric(18, 0)), CAST(100000 AS Numeric(18, 0)), NULL, NULL, N'S', NULL, NULL, CAST(2000 AS Numeric(18, 0)), CAST(70 AS Numeric(18, 0)), N'50/100', N'$50,000 each person', N'$100,000 each accident', 1, CAST(0x9CE1040F AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(5030010 AS Numeric(18, 0)), N'PD ', N'Property Damage Liability', CAST(10000 AS Numeric(18, 0)), NULL, NULL, NULL, N'S', NULL, NULL, CAST(2000 AS Numeric(18, 0)), CAST(10 AS Numeric(18, 0)), N'10', N'', N'', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(5030011 AS Numeric(18, 0)), N'PD ', N'Property Damage Liability', CAST(10000 AS Numeric(18, 0)), NULL, NULL, NULL, N'D', NULL, NULL, CAST(2000 AS Numeric(18, 0)), CAST(20 AS Numeric(18, 0)), N'10', N'', N'', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(5030030 AS Numeric(18, 0)), N'PD ', N'Property Damage Liability', CAST(25000 AS Numeric(18, 0)), NULL, NULL, NULL, N'S', NULL, NULL, CAST(3000 AS Numeric(18, 0)), CAST(10 AS Numeric(18, 0)), N'25', N'', N'', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(5030031 AS Numeric(18, 0)), N'PD ', N'Property Damage Liability', CAST(25000 AS Numeric(18, 0)), NULL, NULL, NULL, N'D', NULL, NULL, CAST(3000 AS Numeric(18, 0)), CAST(20 AS Numeric(18, 0)), N'25', N'', N'', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(5030060 AS Numeric(18, 0)), N'PD ', N'Property Damage Liability', CAST(50000 AS Numeric(18, 0)), NULL, NULL, NULL, N'S', NULL, NULL, CAST(3000 AS Numeric(18, 0)), CAST(50 AS Numeric(18, 0)), N'50', N'', N'', 1, CAST(0x9CE1040F AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(6040010 AS Numeric(18, 0)), N'UBI', N'Uninsured Motorist - BI', CAST(10000 AS Numeric(18, 0)), CAST(20000 AS Numeric(18, 0)), NULL, NULL, N'S', NULL, NULL, CAST(4000 AS Numeric(18, 0)), CAST(10 AS Numeric(18, 0)), N'10/20', N'  ', N' ', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(6040011 AS Numeric(18, 0)), N'UBI', N'Uninsured Motorist - BI', CAST(10000 AS Numeric(18, 0)), CAST(20000 AS Numeric(18, 0)), NULL, NULL, N'D', NULL, NULL, CAST(4000 AS Numeric(18, 0)), CAST(20 AS Numeric(18, 0)), N'10/20', N'  ', N' ', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(6040030 AS Numeric(18, 0)), N'UBI', N'Uninsured Motorist - BI', CAST(25000 AS Numeric(18, 0)), CAST(50000 AS Numeric(18, 0)), NULL, NULL, N'S', NULL, NULL, CAST(4000 AS Numeric(18, 0)), CAST(30 AS Numeric(18, 0)), N'25/50', N'', N'', 1, CAST(0x9B8B028F AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(6040031 AS Numeric(18, 0)), N'UBI', N'Uninsured Motorist - BI', CAST(25000 AS Numeric(18, 0)), CAST(50000 AS Numeric(18, 0)), NULL, NULL, N'D', NULL, NULL, CAST(4000 AS Numeric(18, 0)), CAST(40 AS Numeric(18, 0)), N'25/50', N'', N'', 1, CAST(0x9B8B028F AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(7050010 AS Numeric(18, 0)), N'UPD', N'Uninsured Motorist - PD', CAST(10000 AS Numeric(18, 0)), NULL, NULL, NULL, N'S', NULL, NULL, CAST(5000 AS Numeric(18, 0)), CAST(10 AS Numeric(18, 0)), N'10', N'  ', N' ', 1, CAST(0x9C040000 AS SmallDateTime))
INSERT [dbo].[PgmCovDetail_CV] ([CovCode], [CovGroup], [CovName], [UnitLimit], [LineLimit], [Deductible], [Deductible2], [ShiftType], [LowerRange], [UpperRange], [LowOrderDispl], [HighOrderDispl], [ShortDesc], [Desc1], [Desc2], [PolicyLevel], [InsertDT]) VALUES (CAST(7050011 AS Numeric(18, 0)), N'UPD', N'Uninsured Motorist - PD', CAST(10000 AS Numeric(18, 0)), NULL, NULL, NULL, N'D', NULL, NULL, CAST(5000 AS Numeric(18, 0)), CAST(20 AS Numeric(18, 0)), N'10', N'  ', N' ', 1, CAST(0x9C040000 AS SmallDateTime))
