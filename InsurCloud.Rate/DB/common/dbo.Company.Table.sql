USE [Common]
GO
/****** Object:  Table [dbo].[Company]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Company](
	[CompanyCode] [char](3) NOT NULL,
	[CompanyName] [varchar](90) NULL,
	[CompanyGroupCode] [char](3) NULL,
	[AccountingGroupCode] [char](3) NULL,
	[Address1] [varchar](40) NULL,
	[Address2] [varchar](40) NULL,
	[City] [varchar](30) NULL,
	[Zip] [varchar](20) NULL,
	[StateCode] [char](3) NULL,
	[PhoneNo] [varchar](20) NULL,
	[FaxNo] [varchar](20) NULL,
	[EMail] [varchar](100) NULL,
	[FederalIDNo] [varchar](11) NULL,
	[ISONo] [varchar](5) NULL,
	[NAIINo] [varchar](5) NULL,
	[NCCINo] [varchar](5) NULL,
	[NAICNo] [varchar](5) NULL,
	[AgentDisbSubSystemCode] [char](1) NULL,
	[AgDisbTransactionAccountCode] [char](3) NULL,
	[AgentDisbTransactionTypeCode] [char](3) NULL,
	[PolicyDisbSubSystemCode] [char](1) NULL,
	[PolDisbTransactionAccountCode] [char](3) NULL,
	[PolicyDisbTransactionTypeCode] [char](3) NULL,
	[PremiumSubSystemCode] [char](1) NULL,
	[PremiumTransactionAccountCode] [char](3) NULL,
	[PremiumTransactionTypeCode] [char](3) NULL,
	[InsuredEntityTypeCode] [char](3) NULL,
	[MailToEntityTypeCode] [char](3) NULL,
	[PayeeEntityTypeCode] [char](3) NULL,
	[ClaimantEntityTypeCode] [char](3) NULL,
	[EmailSubject] [varchar](90) NULL,
	[EmailBody] [text] NULL,
	[ReserveEntryModeInd] [char](1) NULL,
	[ReEstablishReserveOnRevCheckYN] [bit] NULL,
	[ClaimReviewDiaryTypeCode] [char](3) NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
	[AAISNo] [varchar](5) NULL,
	[ClaimContactEntityTypeCode] [char](3) NULL,
	[PMReviewDiaryTypeCode] [char](3) NULL,
	[PMReviewDiaryDesc] [varchar](90) NULL,
	[BankAccountProgramBasedYN] [bit] NULL,
	[AgentPaymentPerProgramYN] [bit] NULL,
	[MGAISOCode] [varchar](3) NULL,
	[DoNotApplyAgentLicenseRulesYN] [bit] NULL,
	[DoNotApplyIPLicenseRulesYN] [bit] NULL,
 CONSTRAINT [PK_Company] PRIMARY KEY CLUSTERED 
(
	[CompanyCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[Company] ([CompanyCode], [CompanyName], [CompanyGroupCode], [AccountingGroupCode], [Address1], [Address2], [City], [Zip], [StateCode], [PhoneNo], [FaxNo], [EMail], [FederalIDNo], [ISONo], [NAIINo], [NCCINo], [NAICNo], [AgentDisbSubSystemCode], [AgDisbTransactionAccountCode], [AgentDisbTransactionTypeCode], [PolicyDisbSubSystemCode], [PolDisbTransactionAccountCode], [PolicyDisbTransactionTypeCode], [PremiumSubSystemCode], [PremiumTransactionAccountCode], [PremiumTransactionTypeCode], [InsuredEntityTypeCode], [MailToEntityTypeCode], [PayeeEntityTypeCode], [ClaimantEntityTypeCode], [EmailSubject], [EmailBody], [ReserveEntryModeInd], [ReEstablishReserveOnRevCheckYN], [ClaimReviewDiaryTypeCode], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode], [AAISNo], [ClaimContactEntityTypeCode], [PMReviewDiaryTypeCode], [PMReviewDiaryDesc], [BankAccountProgramBasedYN], [AgentPaymentPerProgramYN], [MGAISOCode], [DoNotApplyAgentLicenseRulesYN], [DoNotApplyIPLicenseRulesYN]) VALUES (N'AP ', N'Apex Lloyds', N'003', N'EOM', N'Paragon Insurance Managers', N'14800 Quorum Drive, Suite 250', N'Dallas', N'75254', N'TX ', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'A', N'CDS', N'CDS', N'A', N'PRP', N'PRM', N'P', N'PRM', N'PRM', N'I  ', N'MT ', N'PAY', N'CLM', NULL, NULL, NULL, 0, N'NCL', CAST(0x00009E6401043BEC AS DateTime), N'MARKWDEPPERSCHMIDT            ', NULL, NULL, NULL, N'CCP', N'016', N'Apex Lloyds default for subro recoveries', 0, 0, NULL, 0, 0)
INSERT [dbo].[Company] ([CompanyCode], [CompanyName], [CompanyGroupCode], [AccountingGroupCode], [Address1], [Address2], [City], [Zip], [StateCode], [PhoneNo], [FaxNo], [EMail], [FederalIDNo], [ISONo], [NAIINo], [NCCINo], [NAICNo], [AgentDisbSubSystemCode], [AgDisbTransactionAccountCode], [AgentDisbTransactionTypeCode], [PolicyDisbSubSystemCode], [PolDisbTransactionAccountCode], [PolicyDisbTransactionTypeCode], [PremiumSubSystemCode], [PremiumTransactionAccountCode], [PremiumTransactionTypeCode], [InsuredEntityTypeCode], [MailToEntityTypeCode], [PayeeEntityTypeCode], [ClaimantEntityTypeCode], [EmailSubject], [EmailBody], [ReserveEntryModeInd], [ReEstablishReserveOnRevCheckYN], [ClaimReviewDiaryTypeCode], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode], [AAISNo], [ClaimContactEntityTypeCode], [PMReviewDiaryTypeCode], [PMReviewDiaryDesc], [BankAccountProgramBasedYN], [AgentPaymentPerProgramYN], [MGAISOCode], [DoNotApplyAgentLicenseRulesYN], [DoNotApplyIPLicenseRulesYN]) VALUES (N'HS ', N'Home State County Mutual Insurance Company', N'002', N'EOM', N'P O Box 702507', NULL, N'Dallas', N'75370-2507', N'TX ', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'29297', N'A', N'CDS', N'CDS', N'A', N'PRP', N'PRM', N'P', N'PRM', N'PRM', N'I  ', N'MT ', N'PAY', N'CLM', NULL, NULL, NULL, 0, N'NCL', CAST(0x00009C1B00DC7DB4 AS DateTime), N'BRENDABORDELON                ', NULL, NULL, NULL, N'CCP', N'016', N'Renewal', 1, 1, NULL, 1, 1)
INSERT [dbo].[Company] ([CompanyCode], [CompanyName], [CompanyGroupCode], [AccountingGroupCode], [Address1], [Address2], [City], [Zip], [StateCode], [PhoneNo], [FaxNo], [EMail], [FederalIDNo], [ISONo], [NAIINo], [NCCINo], [NAICNo], [AgentDisbSubSystemCode], [AgDisbTransactionAccountCode], [AgentDisbTransactionTypeCode], [PolicyDisbSubSystemCode], [PolDisbTransactionAccountCode], [PolicyDisbTransactionTypeCode], [PremiumSubSystemCode], [PremiumTransactionAccountCode], [PremiumTransactionTypeCode], [InsuredEntityTypeCode], [MailToEntityTypeCode], [PayeeEntityTypeCode], [ClaimantEntityTypeCode], [EmailSubject], [EmailBody], [ReserveEntryModeInd], [ReEstablishReserveOnRevCheckYN], [ClaimReviewDiaryTypeCode], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode], [AAISNo], [ClaimContactEntityTypeCode], [PMReviewDiaryTypeCode], [PMReviewDiaryDesc], [BankAccountProgramBasedYN], [AgentPaymentPerProgramYN], [MGAISOCode], [DoNotApplyAgentLicenseRulesYN], [DoNotApplyIPLicenseRulesYN]) VALUES (N'IF ', N'Imperial Fire & Casualty', N'001', N'EOM', N'4670 I-49 N. Service Road', NULL, N'Opelousas', N'70570', N'LA ', N'337-942-5691', NULL, NULL, NULL, NULL, NULL, NULL, N'44369', N'A', N'CDS', N'CDS', N'A', N'PRP', N'PRM', N'P', N'PRM', N'PRM', N'I  ', N'MT ', N'PAY', N'CLM', NULL, NULL, N'C', 0, N'NCL', CAST(0x0000971400E88C6C AS DateTime), N'MARKWDEPPERSCHMIDT            ', CAST(0x00009EFD00E03C4C AS DateTime), N'BRENDABORDELON                ', NULL, N'CCP', N'016', N'Renewal', 1, 1, NULL, 1, 1)
INSERT [dbo].[Company] ([CompanyCode], [CompanyName], [CompanyGroupCode], [AccountingGroupCode], [Address1], [Address2], [City], [Zip], [StateCode], [PhoneNo], [FaxNo], [EMail], [FederalIDNo], [ISONo], [NAIINo], [NCCINo], [NAICNo], [AgentDisbSubSystemCode], [AgDisbTransactionAccountCode], [AgentDisbTransactionTypeCode], [PolicyDisbSubSystemCode], [PolDisbTransactionAccountCode], [PolicyDisbTransactionTypeCode], [PremiumSubSystemCode], [PremiumTransactionAccountCode], [PremiumTransactionTypeCode], [InsuredEntityTypeCode], [MailToEntityTypeCode], [PayeeEntityTypeCode], [ClaimantEntityTypeCode], [EmailSubject], [EmailBody], [ReserveEntryModeInd], [ReEstablishReserveOnRevCheckYN], [ClaimReviewDiaryTypeCode], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode], [AAISNo], [ClaimContactEntityTypeCode], [PMReviewDiaryTypeCode], [PMReviewDiaryDesc], [BankAccountProgramBasedYN], [AgentPaymentPerProgramYN], [MGAISOCode], [DoNotApplyAgentLicenseRulesYN], [DoNotApplyIPLicenseRulesYN]) VALUES (N'NT ', N'National Automotive', N'004', N'EOM', N'111 Veterans Memorial Blvd', N'Suite 1420', N'Metairie', N'70005', N'LA ', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'37486', N'A', N'CDS', N'CDS', N'A', N'PRP', N'PRM', N'P', N'PRM', N'PRM', N'I  ', N'MT ', N'PAY', N'CLM', NULL, NULL, N'C', 0, N'NCL', CAST(0x0000A26C00998400 AS DateTime), N'BRENDABORDELON                ', NULL, NULL, NULL, N'CCP', N'016', N'Renewal', 1, 1, NULL, 1, 1)
ALTER TABLE [dbo].[Company] ADD  DEFAULT (0) FOR [ReEstablishReserveOnRevCheckYN]
GO
ALTER TABLE [dbo].[Company] ADD  DEFAULT (0) FOR [BankAccountProgramBasedYN]
GO
ALTER TABLE [dbo].[Company] ADD  DEFAULT (0) FOR [AgentPaymentPerProgramYN]
GO
ALTER TABLE [dbo].[Company] ADD  DEFAULT (0) FOR [DoNotApplyAgentLicenseRulesYN]
GO
ALTER TABLE [dbo].[Company] ADD  DEFAULT (0) FOR [DoNotApplyIPLicenseRulesYN]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_AccountingGroup] FOREIGN KEY([AccountingGroupCode])
REFERENCES [dbo].[AccountingGroup] ([AccountingGroupCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_AccountingGroup]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_CompanyGroup] FOREIGN KEY([CompanyGroupCode])
REFERENCES [dbo].[CompanyGroup] ([CompanyGroupCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_CompanyGroup]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_DiaryType] FOREIGN KEY([ClaimReviewDiaryTypeCode])
REFERENCES [dbo].[DiaryType] ([DiaryTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_DiaryType]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_DiaryType1] FOREIGN KEY([PMReviewDiaryTypeCode])
REFERENCES [dbo].[DiaryType] ([DiaryTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_DiaryType1]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_EntityType] FOREIGN KEY([MailToEntityTypeCode])
REFERENCES [dbo].[EntityType] ([EntityTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_EntityType]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_EntityType1] FOREIGN KEY([InsuredEntityTypeCode])
REFERENCES [dbo].[EntityType] ([EntityTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_EntityType1]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_EntityType2] FOREIGN KEY([ClaimantEntityTypeCode])
REFERENCES [dbo].[EntityType] ([EntityTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_EntityType2]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_EntityType3] FOREIGN KEY([PayeeEntityTypeCode])
REFERENCES [dbo].[EntityType] ([EntityTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_EntityType3]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_EntityType4] FOREIGN KEY([ClaimContactEntityTypeCode])
REFERENCES [dbo].[EntityType] ([EntityTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_EntityType4]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_State] FOREIGN KEY([StateCode])
REFERENCES [dbo].[State] ([StateCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_State]
GO
ALTER TABLE [dbo].[Company]  WITH CHECK ADD  CONSTRAINT [FK_Company_TransactionType] FOREIGN KEY([AgentDisbSubSystemCode], [AgDisbTransactionAccountCode], [AgentDisbTransactionTypeCode])
REFERENCES [dbo].[TransactionType] ([SubSystemCode], [TransactionAccountCode], [TransactionTypeCode])
GO
ALTER TABLE [dbo].[Company] CHECK CONSTRAINT [FK_Company_TransactionType]
GO
