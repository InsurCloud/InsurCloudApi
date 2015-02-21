USE [Common]
GO
/****** Object:  Table [dbo].[ImportQueue]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ImportQueue](
	[QuoteID] [numeric](18, 0) NOT NULL,
	[PolicyID] [varchar](15) NULL,
	[ProgramCode] [char](3) NULL,
	[CompanyCode] [char](3) NULL,
	[PolicyTransactionNum] [numeric](2, 0) NULL,
	[TermEffDate] [datetime] NULL,
	[CashReceiptNum] [varchar](50) NULL,
	[PolicyXML] [xml] NULL,
	[UserID] [varchar](50) NULL,
	[SystemTS] [datetime] NULL,
 CONSTRAINT [PK_ImportQueue] PRIMARY KEY CLUSTERED 
(
	[QuoteID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
