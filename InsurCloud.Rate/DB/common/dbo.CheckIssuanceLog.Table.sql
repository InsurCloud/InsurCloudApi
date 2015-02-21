USE [Common]
GO
/****** Object:  Table [dbo].[CheckIssuanceLog]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CheckIssuanceLog](
	[CheckIssuanceLogID] [int] NOT NULL,
	[CheckBatchNum] [int] NOT NULL,
	[StartCheckNum] [varchar](50) NULL,
	[EndCheckNum] [varchar](50) NULL,
	[TotalNumOfChecks] [int] NOT NULL,
	[Status] [varchar](150) NOT NULL,
	[NumOfChecksQueued] [int] NOT NULL,
	[QueuedDateT] [datetime] NULL,
	[QueuedUserCode] [varchar](150) NOT NULL,
	[NumOfChecksPrinted] [int] NULL,
	[PrintedDateT] [datetime] NULL,
 CONSTRAINT [PK_CheckIssuanceLog] PRIMARY KEY CLUSTERED 
(
	[CheckIssuanceLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Index [IX_CheckIssuanceLog]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_CheckIssuanceLog] ON [dbo].[CheckIssuanceLog]
(
	[CheckBatchNum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
