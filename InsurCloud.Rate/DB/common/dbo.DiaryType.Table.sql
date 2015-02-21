USE [Common]
GO
/****** Object:  Table [dbo].[DiaryType]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DiaryType](
	[DiaryTypeCode] [char](3) NOT NULL,
	[DiaryTypeDesc] [varchar](90) NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
 CONSTRAINT [PK_DiaryType] PRIMARY KEY CLUSTERED 
(
	[DiaryTypeCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'001', N'Address/Phone Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'002', N'Bill Inquiry', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'003', N'Cancellation', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'004', N'Coverage Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'005', N'Coverage Inquiry', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'006', N'Dealer Inquiry', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'007', N'Driver Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'008', N'Driving Record Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'009', N'Duplicate Policy', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'010', N'Issue Fililng', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'011', N'Lender Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'012', N'Mileage Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'013', N'Name Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'014', N'Quote', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'015', N'Reinstatement', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'016', N'Renewal', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'017', N'Vehicle Change', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'018', N'Verifications', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'019', N'Other', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'020', N'App and Backup', CAST(0x000098F100C0D668 AS DateTime), N'BRENDABORDELON                ', CAST(0x000099C9010CCDAC AS DateTime), N'BRENDABORDELON                ')
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'021', N'POP Backup', CAST(0x000098F100C0E5A4 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'022', N'H O Backup', CAST(0x000098F100C0F030 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'023', N'Non Renew', CAST(0x000098F100C0F990 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'024', N'NSF Check', CAST(0x000098F100C1041C AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'025', N'Agent of Record Letter', CAST(0x000098F100C10FD4 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'026', N'Renewal Offer Updates', CAST(0x000098F100C11DE4 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'EXC', N'Existing Claim Diary', CAST(0x000097B800FB5A7C AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'NCL', N'New Claim - Adjuster Review Required', NULL, NULL, NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'RRS', N'Review Request from Supervisor', CAST(0x000097B800FBB38C AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[DiaryType] ([DiaryTypeCode], [DiaryTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'UNR', N'Underwriting Review Required', NULL, NULL, NULL, NULL)
