USE [Common]
GO
/****** Object:  Table [dbo].[PFCReconcile]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PFCReconcile](
	[ReconcileId] [numeric](15, 0) NOT NULL,
	[Status] [varchar](10) NOT NULL,
	[BankPostedDate] [datetime] NULL,
	[PolicyId] [varchar](20) NOT NULL,
	[TransactionAmt] [decimal](16, 2) NULL,
	[ContractNum] [varchar](20) NULL,
	[TransactionSource] [varchar](20) NOT NULL,
	[AgentCode] [varchar](20) NOT NULL,
	[NachaTransactionReturnCode] [varchar](10) NOT NULL,
	[FtpFileName] [varchar](20) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_PFCReconcile] PRIMARY KEY CLUSTERED 
(
	[ReconcileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [uc_PersonID] UNIQUE NONCLUSTERED 
(
	[ContractNum] ASC,
	[AgentCode] ASC,
	[FtpFileName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
