USE [pgm242]
GO
/****** Object:  Table [dbo].[ErrorLog]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ErrorLog](
	[Program] [varchar](50) NULL,
	[QuoteID] [varchar](50) NULL,
	[PolicyID] [varchar](50) NULL,
	[StartTS] [datetime] NULL,
	[EndTS] [datetime] NULL,
	[AgencyID] [varchar](50) NULL,
	[MethodName] [varchar](100) NULL,
	[ErrorMsg] [varchar](2000) NULL,
	[LogXML] [xml] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
