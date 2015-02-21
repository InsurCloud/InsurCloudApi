/****** Object:  Table [dbo].[PrintRules]    Script Date: 7/26/2014 4:43:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PrintRules](
	[Program] [varchar](10) NOT NULL,
	[ItemGroup] [varchar](50) NOT NULL,
	[ItemCode] [varchar](50) NOT NULL,
	[ItemSubCode] [varchar](50) NOT NULL,
	[ItemValue] [varchar](300) NOT NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[UserId] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_PrintRules] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[ItemGroup] ASC,
	[ItemCode] ASC,
	[ItemSubCode] ASC,
	[ItemValue] ASC,
	[AppliesToCode] ASC,
	[EffDate] ASC,
	[ExpDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'Eliens', N'', N'1', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D4600328489 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'Cancellation', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46016DF885 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'Cancellation', N'LH', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46016DF886 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'DEC', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46001EE755 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'DEC', N'LH', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46001EE757 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'IDCard', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D4700307B55 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'NonRenewal', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D470023E129 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'NonRenewal', N'LH', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D470023E12B AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'PolicyInvoice', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46016E30E6 AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'Reinstatement', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46016E16FD AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'Reinstatement', N'LH', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D46016E16FD AS DateTime))
INSERT [dbo].[PrintRules] ([Program], [ItemGroup], [ItemCode], [ItemSubCode], [ItemValue], [AppliesToCode], [EffDate], [ExpDate], [UserId], [SystemTS]) VALUES (N'All', N'DocTypeRules', N'RecipientType', N'RenewalQuote', N'I', N'B', CAST(0x00009CF100000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), N'shaun.herschbach', CAST(0x00009D9600C47F83 AS DateTime))
