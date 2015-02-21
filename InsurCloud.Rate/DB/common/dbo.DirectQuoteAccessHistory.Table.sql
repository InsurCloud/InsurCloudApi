USE [Common]
GO
/****** Object:  Table [dbo].[DirectQuoteAccessHistory]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DirectQuoteAccessHistory](
	[ID] [numeric](18, 0) NOT NULL,
	[QuoteID] [numeric](18, 0) NOT NULL,
	[DateAccessed] [datetime] NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_DirectQuoteAccessHistory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
