USE [Common]
GO
/****** Object:  Table [dbo].[CitizensCreditCheck]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CitizensCreditCheck](
	[RecordID] [int] IDENTITY(1,1) NOT NULL,
	[CitizensQuoteID] [varchar](50) NULL,
	[CitizensPolicyID] [varchar](50) NULL,
	[ImperialQuoteID] [varchar](50) NULL,
	[QuoteType] [varchar](50) NULL,
	[FullTermPremium] [decimal](18, 2) NULL,
	[TotalFees] [decimal](18, 2) NULL,
	[SystemTS] [datetime] NULL,
 CONSTRAINT [PK_CitizensCreditCheck] PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
