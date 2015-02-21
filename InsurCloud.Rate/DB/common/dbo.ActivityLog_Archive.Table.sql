USE [Common]
GO
/****** Object:  Table [dbo].[ActivityLog_Archive]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ActivityLog_Archive](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[Process] [varchar](50) NOT NULL,
	[StartTS] [datetime] NOT NULL,
	[EndTS] [datetime] NULL,
	[RunDate] [datetime] NULL,
	[Parm01] [varchar](50) NULL,
	[Parm02] [varchar](50) NULL,
	[Parm03] [varchar](50) NULL,
	[Parm04] [varchar](50) NULL,
	[Parm05] [varchar](50) NULL,
	[ArgumentsXML] [xml] NULL,
	[ItemCount] [numeric](18, 0) NOT NULL,
	[System] [varchar](50) NULL,
	[Status] [nchar](10) NULL,
	[Msg] [varchar](2000) NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
