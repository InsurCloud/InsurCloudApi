USE [Common]
GO
/****** Object:  Table [dbo].[DelayedInvoicingWeekOf20140302]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DelayedInvoicingWeekOf20140302](
	[PolicyID] [varchar](25) NULL,
	[Product] [varchar](1) NULL,
	[StateCode] [varchar](2) NULL,
	[InstallmentNum] [varchar](50) NULL,
	[BillDate] [datetime] NULL,
	[DueDate] [datetime] NULL,
	[InvoiceMsgID] [int] NULL,
	[InvoiceProcessedDate] [datetime] NULL,
	[MailMsgID] [int] NULL,
	[MailProcessedDate] [datetime] NULL,
	[DateMailed] [datetime] NULL,
	[LateFeeMsgID] [int] NULL,
	[LateFeeMsgEffDateBefore] [datetime] NULL,
	[LateFeeMsgEffDateAfter] [datetime] NULL,
	[CnxPendMsgID] [int] NULL,
	[CnxPendMsgEffDateBefore] [datetime] NULL,
	[CnxPendMsgEffDateAfter] [datetime] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
