USE [Common]
GO
/****** Object:  Table [dbo].[State]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[State](
	[StateCode] [varchar](2) NOT NULL,
	[Name] [varchar](25) NOT NULL,
	[Abbreviation] [varchar](2) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'01', N'Alabama', N'AL')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'10', N'Georgia', N'GA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'09', N'Florida', N'FL')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'52', N'Hawaii', N'HI')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'11', N'Idaho', N'ID')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'12', N'Illinois', N'IL')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'13', N'Indiana', N'IN')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'14', N'Iowa', N'IA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'15', N'Kansas', N'KS')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'16', N'Kentucky', N'KY')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'17', N'Louisiana', N'LA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'01', N'Alaska', N'AK')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'18', N'Maine', N'ME')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'19', N'Maryland', N'MD')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'20', N'Massachusetts', N'MA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'21', N'Michigan', N'MI')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'22', N'Minnesota', N'MN')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'23', N'Mississippi', N'MS')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'24', N'Missouri', N'MO')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'25', N'Montana', N'MT')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'26', N'Nebraska', N'NE')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'27', N'Nevada', N'NV')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'02', N'Arizona', N'AZ')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'28', N'New Hampshire', N'NH')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'29', N'New Jersey', N'NJ')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'30', N'New Mexico', N'NM')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'31', N'New York', N'NY')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'32', N'North Carolina', N'NC')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'33', N'North Dakota', N'ND')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'34', N'Ohio', N'OH')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'35', N'Oklahoma', N'OK')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'36', N'Oregon', N'OR')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'37', N'Pennsylvania', N'PA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'03', N'Arkansas', N'AR')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'38', N'Rhode Island', N'RI')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'39', N'South Carolina', N'SC')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'40', N'South Dakota', N'SD')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'41', N'Tennessee', N'TN')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'42', N'Texas', N'TX')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'43', N'Utah', N'UT')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'44', N'Vermont', N'VT')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'45', N'Virginia', N'VA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'46', N'Washington', N'WA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'47', N'West Virginia', N'WV')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'04', N'California', N'CA')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'48', N'Wisconsin', N'WI')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'49', N'Wyoming', N'WY')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'05', N'Colorado', N'CO')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'06', N'Connecticut', N'CT')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'08', N'D.C.', N'DC')
INSERT [dbo].[State] ([StateCode], [Name], [Abbreviation]) VALUES (N'07', N'Delaware', N'DE')
