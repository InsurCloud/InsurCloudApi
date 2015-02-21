USE [Common]
GO
/****** Object:  Table [dbo].[ActivityLog]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ActivityLog](
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
	[Msg] [varchar](max) NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_ActivityLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_ACTIVITYLOG_1]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_ACTIVITYLOG_1] ON [dbo].[ActivityLog]
(
	[Parm01] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_ActivityLog_2]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_ActivityLog_2] ON [dbo].[ActivityLog]
(
	[SystemTS] ASC,
	[Process] ASC,
	[System] ASC,
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
