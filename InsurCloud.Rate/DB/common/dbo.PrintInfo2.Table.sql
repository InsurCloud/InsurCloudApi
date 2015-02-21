USE [Common]
GO
/****** Object:  Table [dbo].[PrintInfo2]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PrintInfo2](
	[Program] [varchar](10) NOT NULL,
	[PrintGroup] [varchar](50) NOT NULL,
	[PrintCode] [varchar](50) NOT NULL,
	[PrintSubCode] [varchar](50) NOT NULL,
	[PrintValue] [varchar](max) NOT NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[PrintInfo2] ([Program], [PrintGroup], [PrintCode], [PrintSubCode], [PrintValue], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'ALL', N'CLASS', N'VERSION', N'Check', N'_1', N'B', CAST(0x0000901A00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'kevin.burger', CAST(0x00009F1100D26C8F AS DateTime))
INSERT [dbo].[PrintInfo2] ([Program], [PrintGroup], [PrintCode], [PrintSubCode], [PrintValue], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'ALL', N'PATH', N'FILEPATH', N'FilePath', N'\\dalfs02a\checkprinting\', N'B', CAST(0x00008EAC00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A021010F53F5 AS DateTime))
INSERT [dbo].[PrintInfo2] ([Program], [PrintGroup], [PrintCode], [PrintSubCode], [PrintValue], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'ALL', N'PATH', N'FTPFILEPATH', N'FTPFilePath', N'\\dalapp08\POSimages\FTP\Policy\PAS\', N'B', CAST(0x00008EAC00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A021010F53FC AS DateTime))
