USE [pgm242]
GO
/****** Object:  Table [dbo].[PolicyXML]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PolicyXML](
	[QuoteID] [numeric](18, 0) IDENTITY(24200000000000,1) NOT NULL,
	[PolicyID] [nvarchar](50) NULL,
	[Product] [numeric](18, 0) NULL,
	[State] [numeric](18, 0) NULL,
	[AgencyID] [nchar](10) NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[SavedDate] [datetime] NULL,
	[CreditOrdered] [bit] NULL,
	[CreditMsg] [nvarchar](max) NULL,
	[Premium] [numeric](18, 0) NULL,
	[Fees] [numeric](18, 0) NULL,
	[Status] [varchar](50) NULL,
	[PolicyXML] [xml] NULL,
	[UploadXML] [xml] NULL,
	[BridgeXML] [xml] NULL,
	[UploadTS] [datetime] NULL,
	[StartDate] [datetime] NULL,
 CONSTRAINT [PK_PolicyXML] PRIMARY KEY CLUSTERED 
(
	[QuoteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
