USE [pgm242]
GO
/****** Object:  Table [dbo].[EventLog]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[EventLog](
	[LogItemID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[QuoteID] [varchar](20) NULL,
	[PolicyID] [varchar](20) NULL,
	[FilePath] [varchar](70) NULL,
	[StartTS] [datetime] NULL,
	[EndTS] [datetime] NULL,
	[Premium] [decimal](18, 0) NULL,
	[Fees] [decimal](18, 2) NULL,
	[AgencyID] [varchar](50) NULL,
	[UserID] [varchar](50) NULL,
	[FirstName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[LogXML] [xml] NULL,
 CONSTRAINT [PK_EventLog] PRIMARY KEY CLUSTERED 
(
	[LogItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
