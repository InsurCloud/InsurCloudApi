USE [Common]
GO
/****** Object:  Table [dbo].[AccountingReports]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AccountingReports](
	[ReportName] [varchar](50) NOT NULL,
	[Description] [varchar](250) NOT NULL,
	[ExecType] [varchar](50) NOT NULL,
	[ExecQuery] [varchar](max) NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [date] NOT NULL,
 CONSTRAINT [PK_AccountingReports] PRIMARY KEY CLUSTERED 
(
	[ReportName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[AccountingReports] ([ReportName], [Description], [ExecType], [ExecQuery], [UserID], [SystemTS]) VALUES (N'DirectPayCancelESign', N'Cancelled Payments Direct No ESign', N'Query', N'SELECT ''Cancelled''                                                     AS Status,         pay.PolicyNbr                                                   AS PolicyID,         pay.PaymentAmt                                                  AS Amt,         PaymentMethod                                                   AS PaymentMethod,         pay.SystemTS                                                    AS PaymentEnteredDate,         pay.CancelledDate                                               AS CancelledDate,         PaymentMethodXML.value(''(/CreditCard/CardNo)[1]'', ''varchar(4)'') AS Last4ofCC  FROM   Common..payment pay WITH (NOLOCK)         JOIN PasCarrier..Policy pol WITH (NOLOCK)           ON pay.PolicyNbr COLLATE database_Default = pol.PolicyNo              AND pol.PolicyTermTypeInd = ''N''              AND pol.PolicyTransactionNum = ''1''              AND pay.IsSuspended = ''1''         JOIN pgm242..PolCancellation canc WITH (NOLOCK)           ON pay.PolicyNbr = canc.PolicyID              AND canc.CancelCode = ''556''              AND canc.Status IN ( ''Pending'', ''Processed'' )              AND pay.CancelledDate IS NOT NULL              AND pay.CancelledDate <= @ToDate              AND pay.CancelledDate >= @FromDate   union all  SELECT ''Cancelled''                                                    AS Status,         pay.PolicyNbr                                                   AS PolicyID,         pay.PaymentAmt                                                  AS Amt,         PaymentMethod                                                   AS PaymentMethod,         pay.SystemTS                                                    AS PaymentEnteredDate,         pay.CancelledDate                                               AS CancelledDate,         PaymentMethodXML.value(''(/CreditCard/CardNo)[1]'', ''varchar(4)'') AS Last4ofCC  FROM   Common..payment pay WITH (NOLOCK)         JOIN PasCarrier..Policy pol WITH (NOLOCK)           ON pay.PolicyNbr COLLATE database_Default = pol.PolicyNo              AND pol.PolicyTermTypeInd = ''N''              AND pol.PolicyTransactionNum = ''1''              AND pay.IsSuspended = ''1''         JOIN pgm217..PolCancellation canc WITH (NOLOCK)           ON pay.PolicyNbr = canc.PolicyID              AND canc.CancelCode = ''556''              AND canc.Status IN ( ''Pending'', ''Processed'' )              AND pay.CancelledDate IS NOT NULL              AND pay.CancelledDate <= @ToDate              AND pay.CancelledDate >= @FromDate union all  SELECT ''Cancelled''                                                    AS Status,         pay.PolicyNbr                                                   AS PolicyID,         pay.PaymentAmt                                                  AS Amt,         PaymentMethod                                                   AS PaymentMethod,         pay.SystemTS                                                    AS PaymentEnteredDate,         pay.CancelledDate                                               AS CancelledDate,         PaymentMethodXML.value(''(/CreditCard/CardNo)[1]'', ''varchar(4)'') AS Last4ofCC  FROM   Common..payment pay WITH (NOLOCK)         JOIN PasCarrier..Policy pol WITH (NOLOCK)           ON pay.PolicyNbr COLLATE database_Default = pol.PolicyNo              AND pol.PolicyTermTypeInd = ''N''              AND pol.PolicyTransactionNum = ''1''              AND pay.IsSuspended = ''1''         JOIN pgm203..PolCancellation canc WITH (NOLOCK)           ON pay.PolicyNbr = canc.PolicyID              AND canc.CancelCode = ''556''              AND canc.Status IN ( ''Pending'', ''Processed'' )              AND pay.CancelledDate IS NOT NULL              AND pay.CancelledDate <= @ToDate              AND pay.CancelledDate >= @FromDate ', N'diep.nguyen', CAST(0x20370B00 AS Date))
INSERT [dbo].[AccountingReports] ([ReportName], [Description], [ExecType], [ExecQuery], [UserID], [SystemTS]) VALUES (N'DirectPaySuspend', N'Suspended Payments Direct', N'Query', N'SELECT ''Suspended''    AS Status,         pay.PolicyNbr  AS PolicyID,         pay.PaymentAmt AS Amt,         PaymentMethod  AS PaymentMethod,         SystemTS       AS PaymentEnteredDate  FROM   Common..payment pay WITH (NOLOCK)         JOIN PasCarrier..Policy pol WITH (NOLOCK)           ON pay.PolicyNbr COLLATE database_Default = pol.PolicyNo              AND pol.PolicyTermTypeInd = ''N''              AND pol.PolicyTransactionNum = ''1''              AND pay.IsSuspended = ''1''              and pol.ProgramCode in (''TXD'',''LAD'', ''ARD'')              AND pay.SystemTS >= @FromDate              AND pay.SystemTS <= @ToDate              AND pay.CancelledDate IS NULL ', N'diep.nguyen', CAST(0x20370B00 AS Date))
INSERT [dbo].[AccountingReports] ([ReportName], [Description], [ExecType], [ExecQuery], [UserID], [SystemTS]) VALUES (N'PolicyWithNoMoneyPosted', N'Policy with no Money Posted', N'Query', N'Exec common..PolicyWithNoPaymentPosted @FromDate, @ToDate', N'shaun.herschbach', CAST(0xB0360B00 AS Date))
