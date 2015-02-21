/****** Object:  Table [dbo].[UWQuestion]    Script Date: 7/26/2014 4:43:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UWQuestion](
	[Program] [varchar](10) NOT NULL,
	[UWQuestionCode] [varchar](10) NOT NULL,
	[UWQuestionDesc] [varchar](250) NOT NULL,
	[AdverseResponse] [varchar](3) NOT NULL,
	[AdverseAction] [varchar](20) NOT NULL,
	[AdverseActionValue] [varchar](250) NOT NULL,
	[HighOrderDisplay] [int] NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL
 CONSTRAINT [PK_UWQuestion] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[UWQuestionCode] ASC,	
	[AppliesToCode] ASC,
	[EffDate] ASC,
	[ExpDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)


GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'300', N'Are all household residents and vehicle operators age 14 or older listed on the application as either an active driver or as an excluded driver?', N'No', N'Redirect', N'Drivers', 1, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A10900E4F01B AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'301', N'Does any driver have physical or mental impairments? If yes, a Physician’s Statement is required.', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Drivers requiring a Physician''s Statement require company approval.', 2, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14000E37EF6 AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'302', N'Are there any other individuals, resident or not, who regularly use your vehicle?', N'Yes', N'Redirect', N'Drivers', 3, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A10900E4F01B AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'303', N'Does the named insured or his/her spouse own any other vehicles that are not listed on the application?', N'Yes', N'Input', N'Please list the year, make, and model for each vehicle as well as the reason each owned vehicle is not listed on the policy.', 4, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A10B00AD71B8 AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'304', N'Is any vehicle used for business or commercial purposes?', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with Business Use require company approval.', 5, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14000E37F12 AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'305', N'Does any vehicle have sound receiving equipment valued at $500 or more?', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with sound receiving equipment valued at $500 or more require company approval.', 6, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14000E37F16 AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'306', N'Are all vehicles listed on the application garaged at the policy address?', N'No', N'Input', N'Please list the year, make, model and full address for each vehicle not garaged at the policy address.', 7, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A10B00AD71BC AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'307', N'Has any vehicle listed on the application been re-built, salvaged or water damaged?', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles that are re-built, salvaged or water damaged require company approval.', 8, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14000E37F1A AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'308', N'Do any vehicles listed on the application have custom wheels, custom paint or custom body panels?', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with custom equipment require company approval.', 9, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14000E37F1E AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'309', N'Are all vehicles listed on the application registered to the named insured or his/her spouse?', N'No', N'Input', N'Please list the year, make, and model for each vehicle as well as the reason each registered vehicle is not listed on the policy.', 10, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A10B00AD71BF AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'310', N'Do any vehicles listed on the application have existing damage?', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.  Vehicles with existing damage require company approval.', 11, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14000E37F21 AS DateTime))
INSERT [dbo].[UWQuestion] ([Program], [UWQuestionCode], [UWQuestionDesc], [AdverseResponse], [AdverseAction], [AdverseActionValue], [HighOrderDisplay], [AppliesToCode], [EffDate], [ExpDate], [UserID], [SystemTS]) VALUES (N'Direct', N'311', N'Are any of the vehicles or active drivers listed on the application an ineligible risk? <a href="/Purchase/IneligibleRisks" target="_blank">Click here </a> to see a list of ineligible risks.', N'Yes', N'Message', N'Please call 866-874-2741 to speak with an Imperial Representative to complete your application.', 12, N'B', CAST(0x0000A0DD00000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'vikram.paruchuri', CAST(0x0000A14D0106E8A0 AS DateTime))
