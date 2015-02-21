USE [Common]
GO
/****** Object:  Table [dbo].[DirectQuoteLookup]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DirectQuoteLookup](
	[ID] [numeric](18, 0) NOT NULL,
	[Product] [int] NOT NULL,
	[State] [int] NOT NULL,
	[Program] [varchar](10) NOT NULL,
	[QuoteID] [numeric](18, 0) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[DOB] [date] NOT NULL,
	[ZipCode] [varchar](5) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_DirectQuoteLookup] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
