USE [Common]
GO
/****** Object:  Table [dbo].[PgmTransactionCategory]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PgmTransactionCategory](
	[TransactionTypeCode] [varchar](6) NOT NULL,
	[TransactionCategory] [varchar](12) NOT NULL,
	[Description] [varchar](50) NOT NULL,
	[InsertDT] [smalldatetime] NOT NULL,
	[InsertUserId] [varchar](64) NOT NULL,
 CONSTRAINT [PK_PgmTransactionCategory_1] PRIMARY KEY CLUSTERED 
(
	[TransactionTypeCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'ADJ', N'Reversal', N'Reversal', CAST(0x9DAE0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'AIF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'ALF', N'SvcFee', N'Service Fees', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'ANF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'AP', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'ATF', N'AssessFee', N'Assessment Fees', CAST(0x9DFF01A3 AS SmallDateTime), N'AppUser')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'BE ', N'Receipt', N'Cash Receipt', CAST(0x9F9F0285 AS SmallDateTime), N'mindy.arvisu')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'BF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CAD', N'Comm', N'Commission', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CAF', N'AssessFee', N'Assessment Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CAR', N'Comm', N'Commission', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CAT', N'AssessFee', N'Assessment Fees', CAST(0x9D190413 AS SmallDateTime), N'shaun.herschbach')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CC', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CF', N'SvcFee', N'Service Fees', CAST(0x9BB30000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CH', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CKP', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'CSH', N'Receipt', N'Cash Receipt', CAST(0x9BD004C7 AS SmallDateTime), N'pwsa')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'DEP', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'DRE', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'ECK', N'Receipt', N'Cash Receipt', CAST(0x9C1503EA AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'EFT', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'IC ', N'Disburse', N'Disbursement', CAST(0x9D2D0190 AS SmallDateTime), N'AppUser')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'IF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'INS', N'SvcFee', N'Service Fees', CAST(0x9BB30000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'INT', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'JER', N'Premium', N'Premium', CAST(0x9BBD0294 AS SmallDateTime), N'IMPERIAL\dbconsultant1')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'LBX', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'LF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'MIS', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'MO', N'Receipt', N'Cash Receipt', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'MVR', N'SvcFee', N'Service Fees', CAST(0x9BB30000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'NFE', N'SvcFee', N'Service Fees', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'NSF', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'OP', N'Disburse', N'Disbursement', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PF', N'PolFee', N'Policy Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PFC', N'Receipt', N'Premium Finance', CAST(0x9D190413 AS SmallDateTime), N'shaun.herschbach')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PJE', N'Premium', N'Premium', CAST(0x9BBD0294 AS SmallDateTime), N'IMPERIAL\dbconsultant1')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PP', N'Receipt', N'Cash Receipt', CAST(0x9C1503EA AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PPF', N'SvcFee', N'Service Fees', CAST(0x9D1E02AA AS SmallDateTime), N'shaun.herschbach')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PRE', N'Premium', N'Premium', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PRM', N'Premium', N'Premium', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PRP', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'PWO', N'Premium', N'Premium', CAST(0x9BBD0294 AS SmallDateTime), N'IMPERIAL\dbconsultant1')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'RNF', N'SvcFee', N'Service Fees', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'RP', N'Disburse', N'Disbursement', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'S22', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'S26', N'SvcFee', N'Service Fees', CAST(0x9BB30000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'SP', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'SUR', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'VPC', N'Reversal', N'Reversal', CAST(0x9BC80375 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'WBF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'WLF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'WOM', N'Premium', N'Premium', CAST(0x9BBD0294 AS SmallDateTime), N'IMPERIAL\dbconsultant1')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'WPF', N'PolFee', N'Policy Fees', CAST(0x9D360188 AS SmallDateTime), N'AppUser')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'WPP', N'SvcFee', N'Service Fees', CAST(0x9DC1019D AS SmallDateTime), N'AppUser')
INSERT [dbo].[PgmTransactionCategory] ([TransactionTypeCode], [TransactionCategory], [Description], [InsertDT], [InsertUserId]) VALUES (N'WRF', N'SvcFee', N'Service Fees', CAST(0x9BAC0000 AS SmallDateTime), N'scott.proctor')
