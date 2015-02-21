USE [Common]
GO
/****** Object:  Table [dbo].[BatchScheduleActivityLogXRef]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BatchScheduleActivityLogXRef](
	[BatchProcess] [nvarchar](50) NOT NULL,
	[ActivityProcess] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'Print', N'PrintProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'Mail', N'MailProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ArchivePolicyXML', N'ArchiveBatchMsgPolXml')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ArchiveXmlCleanup', N'ArchiveXmlCleanup')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'BatchCompleted', N'BatchCompleted')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'CheckPrint', N'CheckIssuancePrint')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ClaimFreeDiscountRemover', N'ClaimFreeDiscountRemover')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'DisbursementMsgGen', N'DisbursementMsgGen')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'EFTPaymentGen', N'EFTPaymentGeneration')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'EliensFileFinalization', N'EliensFileFinalization')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'EliensProcess', N'EliensProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ENotificationRecon', N'ENotificationRecon')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ESigGetCompleted', N'ESignatureGetCompleted')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ESigGetRegisteredProducers', N'ESignatureGetRegisteredProducers')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'ExpireQuotes', N'ExpQuotePolXMLCleanUp')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'GenerateInvoice', N'GenerateInvoiceBatchProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'GenerateLateFeesProcess', N'GenerateLateFeesProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'GenerateSuspensePrint', N'GenerateSuspensePrint')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'GenSR26ForExpPol', N'GenSR26ForExpPol')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'IMCDevilMonthlyNonRenewal', N'IMCDevilMonthlyNonRenewal')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'IMCDevilMonthlyNonRenewal', N'IMCDevilMonthlyNonRenewal')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'InternalAdjustments', N'InternalAdjustments')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'NSFPosting', N'NSFPosting')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'PaymentPosting', N'PaymentPosting')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'PolicyDisbursements', N'PolicyDisbursements')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'PolicyRollOver', N'PolicyRolloverProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'PreindexingReporter', N'PreindexingReporter')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'RenewalQuoteAudit', N'RenewalQuoteAudit')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'RenewalQuoteEFTBinding', N'RenewalQuoteEFTBinding')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'RenewalQuoteExpiration', N'RenewalQuoteExpiration')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'RenewalQuotePendingAudit', N'RenewalQuotePendingAudit')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'RenewalQuoteProcess', N'RenewalQuoteProcess')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'UnisoftFileRecon', N'UnisoftFileRecon')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'UnisoftFileRetrieval', N'UnisoftFileRetrieval')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'VINMismatchMsgGen', N'VINMismatchMsgGen')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'WriteOff', N'BATCH_WRITEOFF')
INSERT [dbo].[BatchScheduleActivityLogXRef] ([BatchProcess], [ActivityProcess]) VALUES (N'CNXNotice', N'CNXNotice')
