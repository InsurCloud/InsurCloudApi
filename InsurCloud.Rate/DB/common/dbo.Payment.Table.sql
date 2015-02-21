USE [Common]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Payment](
	[PaymentID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[PolicyNbr] [varchar](50) NULL,
	[PaymentMethod] [varchar](50) NOT NULL,
	[PaymentAmt] [decimal](16, 2) NOT NULL,
	[PaymentDate] [datetime] NOT NULL,
	[SweepDate] [datetime] NULL,
	[PostedDate] [datetime] NULL,
	[CancelledDate] [datetime] NULL,
	[Comments] [varchar](3000) NULL,
	[CashReceiptNum] [numeric](18, 0) NULL,
	[PaymentMethodXML] [xml] NULL,
	[UserID] [varchar](50) NULL,
	[SystemTS] [datetime] NULL,
	[ReprocessedDate] [datetime] NULL,
	[ReprocessFlag] [bit] NULL,
	[SourceSystem] [varchar](50) NULL,
	[Type] [varchar](50) NULL,
	[AgencyID] [varchar](50) NULL,
	[IsSuspended] [bit] NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY NONCLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Payment_1]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_1] ON [dbo].[Payment]
(
	[PolicyNbr] ASC,
	[PaymentDate] ASC,
	[SourceSystem] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Payment_2]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_2] ON [dbo].[Payment]
(
	[PolicyNbr] ASC,
	[CancelledDate] ASC,
	[PostedDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_3]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_3] ON [dbo].[Payment]
(
	[SystemTS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Payment_PolicyNbr]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PolicyNbr] ON [dbo].[Payment]
(
	[PolicyNbr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [ix_Payment_PostedDate+CancelledDate+i]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [ix_Payment_PostedDate+CancelledDate+i] ON [dbo].[Payment]
(
	[PostedDate] ASC,
	[CancelledDate] ASC
)
INCLUDE ( 	[PaymentID],
	[PolicyNbr],
	[PaymentMethod],
	[PaymentAmt],
	[PaymentDate],
	[IsSuspended]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentDate]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentDate] ON [dbo].[Payment]
(
	[PaymentDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
