/****** Object:  Table [dbo].[ProgramSettings]    Script Date: 7/26/2014 4:43:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProgramSettings](
	[Program] [varchar](50) NOT NULL,
	[SettingName] [varchar](50) NOT NULL,
	[SettingDesc] [varchar](200) NOT NULL,
	[Value] [varchar](50) NOT NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ProgramSettings_1] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[SettingName] ASC,
	[AppliesToCode] ASC,
	[EffDate] ASC,
	[ExpDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[ProgramSettings] ([Program], [SettingName], [SettingDesc], [Value], [AppliesToCode], [EffDate], [ExpDate]) VALUES (N'Classic', N'MaxDriverPoints', N'Maxmium Driver Points', N'12', N'B', CAST(0x00009A1600000000 AS DateTime), CAST(0x00009F0200000000 AS DateTime))
INSERT [dbo].[ProgramSettings] ([Program], [SettingName], [SettingDesc], [Value], [AppliesToCode], [EffDate], [ExpDate]) VALUES (N'Classic', N'MaxDriverPoints', N'Maxmium Driver Points', N'11', N'B', CAST(0x00009F0200000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime))
INSERT [dbo].[ProgramSettings] ([Program], [SettingName], [SettingDesc], [Value], [AppliesToCode], [EffDate], [ExpDate]) VALUES (N'Direct', N'MaxDriverPoints', N'Maxmium Driver Points', N'11', N'B', CAST(0x00009AB700000000 AS DateTime), CAST(0x0000D76E00000000 AS DateTime))
INSERT [dbo].[ProgramSettings] ([Program], [SettingName], [SettingDesc], [Value], [AppliesToCode], [EffDate], [ExpDate]) VALUES (N'Direct', N'WeatherOverrideDate', N'Weather Override', N'1/23/2014', N'B', CAST(0x0000A13900000000 AS DateTime), CAST(0x0000A13900000000 AS DateTime))
