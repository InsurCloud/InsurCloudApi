USE [Common]
GO
/****** Object:  Table [dbo].[PolicyPfcTransaction]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PolicyPfcTransaction](
	[PolicyId] [varchar](50) NOT NULL,
	[TermEffDate] [datetime] NOT NULL,
	[PolicyTransactionNum] [int] NOT NULL,
	[UnisoftContractNum] [bigint] NOT NULL,
	[UnisoftQuoteNum] [bigint] NULL,
	[PfcEntityNum] [bigint] NULL,
	[Amount] [decimal](18, 2) NULL,
	[FirstPaymentDate] [datetime] NULL,
	[ImageId] [varchar](50) NULL,
	[AgentCode] [varchar](50) NULL,
	[Status] [varchar](50) NULL,
	[ReconcileId] [bigint] NULL,
	[SystemTs] [datetime] NOT NULL,
 CONSTRAINT [PK_PolicyPfcTransaction] PRIMARY KEY CLUSTERED 
(
	[PolicyId] ASC,
	[TermEffDate] ASC,
	[PolicyTransactionNum] ASC,
	[UnisoftContractNum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
