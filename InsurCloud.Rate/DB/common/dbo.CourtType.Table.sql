USE [Common]
GO
/****** Object:  Table [dbo].[CourtType]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CourtType](
	[CourtTypeCode] [char](3) NOT NULL,
	[CourtTypeDesc] [varchar](90) NULL,
	[AddedUserCode] [char](30) NULL,
	[AddedDateT] [datetime] NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
 CONSTRAINT [PK_CourtType] PRIMARY KEY CLUSTERED 
(
	[CourtTypeCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'001', N'City', N'BRENDABORDELON                ', CAST(0x000097C300F45AB0 AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'002', N'District', N'BRENDABORDELON                ', CAST(0x000097C300F462E4 AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'003', N'Federal', N'BRENDABORDELON                ', CAST(0x000097C300F468C0 AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'004', N'State', N'BRENDABORDELON                ', CAST(0x000097C300F46E9C AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'005', N'JP', N'BRENDABORDELON                ', CAST(0x000097C300F475A4 AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'006', N'Small Claims Court', N'BRENDABORDELON                ', CAST(0x000097C300F47DD8 AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'007', N'Appellate', N'BRENDABORDELON                ', CAST(0x000097C300F4860C AS DateTime), NULL, NULL)
INSERT [dbo].[CourtType] ([CourtTypeCode], [CourtTypeDesc], [AddedUserCode], [AddedDateT], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'008', N'Supreme', N'BRENDABORDELON                ', CAST(0x000097C300F49098 AS DateTime), NULL, NULL)
