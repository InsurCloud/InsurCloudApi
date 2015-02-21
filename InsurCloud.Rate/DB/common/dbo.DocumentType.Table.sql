USE [Common]
GO
/****** Object:  Table [dbo].[DocumentType]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DocumentType](
	[DocumentTypeCode] [char](3) NOT NULL,
	[DocumentTypeDesc] [varchar](90) NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
 CONSTRAINT [PK_DocumentType] PRIMARY KEY CLUSTERED 
(
	[DocumentTypeCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'BIL', N'Premium Statement', CAST(0x0000970F0112E1BE AS DateTime), N'admin                         ', CAST(0x0000970F0112E1BE AS DateTime), N'admin                         ')
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'CAN', N'Cancellation', CAST(0x0000970F0112E1DF AS DateTime), N'admin                         ', CAST(0x0000970F0112E1DF AS DateTime), N'admin                         ')
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'CHK', N'Checks/Disbursements', CAST(0x0000970F0112E1FF AS DateTime), N'admin                         ', CAST(0x0000970F0112E1FF AS DateTime), N'admin                         ')
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'DEC', N'Declaration', CAST(0x0000974F0180612C AS DateTime), N'TRACYLANCASTER                ', NULL, NULL)
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'LET', N'Miscellaneous Letters', CAST(0x0000976300B57520 AS DateTime), N'MARKWDEPPERSCHMIDT            ', NULL, NULL)
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'NEW', N'New Business Package for Insured', CAST(0x0000972B00FDC9EC AS DateTime), N'MARKWDEPPERSCHMIDT            ', NULL, NULL)
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'NOR', N'NonRenewal', CAST(0x0000970F0112E1A2 AS DateTime), N'admin                         ', CAST(0x0000970F0112E1A2 AS DateTime), N'admin                         ')
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'REI', N'Reinstatement', CAST(0x0000970F0112E220 AS DateTime), N'admin                         ', CAST(0x0000970F0112E220 AS DateTime), N'admin                         ')
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'REN', N'Renewal quotes & reminders', CAST(0x0000982F00B7258C AS DateTime), N'MARKWDEPPERSCHMIDT            ', NULL, NULL)
INSERT [dbo].[DocumentType] ([DocumentTypeCode], [DocumentTypeDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'UWL', N'Underwriting Letters', CAST(0x0000974F018278B8 AS DateTime), N'TRACYLANCASTER                ', NULL, NULL)
