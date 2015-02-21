USE [Common]
GO
/****** Object:  Table [dbo].[EoMMonths]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EoMMonths](
	[Month] [varchar](6) NOT NULL,
	[BeginDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_EoMMonths] PRIMARY KEY CLUSTERED 
(
	[Month] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201401', CAST(0x0000A2A200000000 AS DateTime), CAST(0x0000A2C400000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3263 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201402', CAST(0x0000A2C500000000 AS DateTime), CAST(0x0000A2E000000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201403', CAST(0x0000A2E100000000 AS DateTime), CAST(0x0000A2FC00000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201404', CAST(0x0000A2FD00000000 AS DateTime), CAST(0x0000A31800000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201405', CAST(0x0000A31900000000 AS DateTime), CAST(0x0000A33B00000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201406', CAST(0x0000A33C00000000 AS DateTime), CAST(0x0000A35700000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201407', CAST(0x0000A35800000000 AS DateTime), CAST(0x0000A37300000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201408', CAST(0x0000A37400000000 AS DateTime), CAST(0x0000A39600000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201409', CAST(0x0000A39700000000 AS DateTime), CAST(0x0000A3B200000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201410', CAST(0x0000A3B300000000 AS DateTime), CAST(0x0000A3D500000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3266 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201411', CAST(0x0000A3D600000000 AS DateTime), CAST(0x0000A3F100000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3267 AS DateTime))
INSERT [dbo].[EoMMonths] ([Month], [BeginDate], [EndDate], [UserID], [SystemTS]) VALUES (N'201412', CAST(0x0000A3F200000000 AS DateTime), CAST(0x0000A40D00000000 AS DateTime), N'shaun.herschbach', CAST(0x0000A2B200EA3267 AS DateTime))
