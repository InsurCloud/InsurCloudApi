USE [Common]
GO
/****** Object:  Table [dbo].[EditBackUp]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EditBackUp](
	[Program] [varchar](10) NOT NULL,
	[Category] [varchar](50) NOT NULL,
	[SubCategory] [varchar](50) NOT NULL,
	[EditCode] [varchar](50) NOT NULL,
	[EditValue] [nvarchar](50) NOT NULL,
	[EditDesc] [varchar](250) NOT NULL,
	[HighOrderDispl] [int] NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[EditBackUp] ([Program], [Category], [SubCategory], [EditCode], [EditValue], [EditDesc], [HighOrderDispl], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'FLC', N'BATCHPROCESS', N'INVOICE', N'1', N'FLC', N'Bypass the PAS Invoice', 1, N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'MARIBEL.REYMUNDO', CAST(0x00009CF5009FB460 AS DateTime))
