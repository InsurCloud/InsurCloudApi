USE [Common]
GO
/****** Object:  Table [dbo].[CheckPrinters]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CheckPrinters](
	[Name] [varchar](25) NOT NULL,
	[Location] [varchar](150) NOT NULL,
	[UserId] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_CheckPrinters] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[CheckPrinters] ([Name], [Location], [UserId], [SystemTS]) VALUES (N'BOSSCHK', N'Bossier Claims', N'shaun.herschbach', CAST(0x0000A23E00C06DCC AS DateTime))
INSERT [dbo].[CheckPrinters] ([Name], [Location], [UserId], [SystemTS]) VALUES (N'LACHK01', N'Dallas Claims', N'shaun.herschbach', CAST(0x0000A22A0116F7FA AS DateTime))
INSERT [dbo].[CheckPrinters] ([Name], [Location], [UserId], [SystemTS]) VALUES (N'LAX', N'Nancy''s office', N'shaun.herschbach', CAST(0x0000A22A0116F7FA AS DateTime))
INSERT [dbo].[CheckPrinters] ([Name], [Location], [UserId], [SystemTS]) VALUES (N'MIA', N'Robert''s office', N'shaun.herschbach', CAST(0x0000A22A0116F7FB AS DateTime))
INSERT [dbo].[CheckPrinters] ([Name], [Location], [UserId], [SystemTS]) VALUES (N'TXCHK01', N'Julie''s office', N'shaun.herschbach', CAST(0x0000A22A0116F7FB AS DateTime))
