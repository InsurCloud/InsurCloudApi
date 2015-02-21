USE [Common]
GO
/****** Object:  Table [dbo].[About]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[About](
	[DBVersionNum] [char](30) NOT NULL,
	[DBName] [char](30) NOT NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[About] ([DBVersionNum], [DBName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'6.2.0                         ', N'Common                    ', CAST(0x0000970F0112BE7A AS DateTime), N'admin                         ', NULL, NULL)
