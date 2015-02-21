USE [Common]
GO
/****** Object:  Table [dbo].[SearchFields]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SearchFields](
	[SearchType] [varchar](50) NOT NULL,
	[FieldName] [varchar](50) NOT NULL,
	[FieldType] [varchar](50) NOT NULL,
	[FieldLength] [numeric](5, 0) NULL,
	[FieldEditCodeValues] [varchar](150) NULL,
	[JoinID] [numeric](5, 0) NULL,
	[Column] [varchar](50) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[HighOrderDispl] [int] NULL,
 CONSTRAINT [PK_SearchFields] PRIMARY KEY CLUSTERED 
(
	[SearchType] ASC,
	[FieldName] ASC,
	[FieldType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver Date of Birth', N'DatePicker', CAST(10 AS Numeric(5, 0)), N'DATE', CAST(2 AS Numeric(5, 0)), N'BirthDate', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 26)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver DL Number', N'TextBox', CAST(12 AS Numeric(5, 0)), N'STRING', CAST(2 AS Numeric(5, 0)), N'DriversLicenseNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 27)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver DL State', N'ComboBox', CAST(2 AS Numeric(5, 0)), N'Policy_AllStates', CAST(2 AS Numeric(5, 0)), N'StateCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 28)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver First Name', N'TextBox', CAST(25 AS Numeric(5, 0)), N'STRING', CAST(2 AS Numeric(5, 0)), N'FirstName', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 24)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver Full Name', N'TextBox', CAST(50 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'EntityName', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 23)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver Last Name', N'TextBox', CAST(25 AS Numeric(5, 0)), N'STRING', CAST(2 AS Numeric(5, 0)), N'LastName', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 25)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Driver SSN', N'TextBox', CAST(9 AS Numeric(5, 0)), N'STRING', CAST(2 AS Numeric(5, 0)), N'SocialSecurityNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 29)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Dwelling Address1', N'TextBox', CAST(30 AS Numeric(5, 0)), N'STRING', CAST(4 AS Numeric(5, 0)), N'Address1', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 35)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Dwelling Address2', N'TextBox', CAST(30 AS Numeric(5, 0)), N'STRING', CAST(4 AS Numeric(5, 0)), N'Address2', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 36)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Dwelling City', N'TextBox', CAST(25 AS Numeric(5, 0)), N'STRING', CAST(4 AS Numeric(5, 0)), N'City', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 37)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Dwelling Phone Number', N'TextBox', CAST(15 AS Numeric(5, 0)), N'STRING', CAST(4 AS Numeric(5, 0)), N'PhoneNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 40)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Dwelling State', N'ComboBox', CAST(2 AS Numeric(5, 0)), N'Policy_AllStates', CAST(4 AS Numeric(5, 0)), N'StateCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 38)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Dwelling Zip Code', N'TextBox', CAST(5 AS Numeric(5, 0)), N'STRING', CAST(4 AS Numeric(5, 0)), N'Zip', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 39)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Address1', N'TextBox', CAST(30 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'Address1', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 17)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Address2', N'TextBox', CAST(30 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'Address2', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 18)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured City', N'TextBox', CAST(25 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'City', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 19)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Date of Birth', N'DatePicker', CAST(10 AS Numeric(5, 0)), N'DATE', CAST(1 AS Numeric(5, 0)), N'DateOfBirth', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 15)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured First Name', N'TextBox', CAST(25 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'FirstName', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 3)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Full Name', N'TextBox', CAST(50 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'EntityName', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 2)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Gender', N'ComboBox', CAST(6 AS Numeric(5, 0)), N'Policy_Gender', CAST(1 AS Numeric(5, 0)), N'GenderInd', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 22)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Home Phone Number', N'TextBox', CAST(15 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'HomePhoneNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 6)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Last Name', N'TextBox', CAST(25 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'LastName', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 4)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Phone Number', N'TextBox', CAST(15 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'PhoneNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 5)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured SSN', N'TextBox', CAST(9 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'SocialSecurityNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 16)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured State', N'ComboBox', CAST(2 AS Numeric(5, 0)), N'Policy_AllStates', CAST(1 AS Numeric(5, 0)), N'StateCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 20)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Insured Zip', N'TextBox', CAST(5 AS Numeric(5, 0)), N'STRING', CAST(1 AS Numeric(5, 0)), N'Zip', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 21)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy Agent ID', N'TextBox', CAST(8 AS Numeric(5, 0)), N'STRING', CAST(0 AS Numeric(5, 0)), N'AgentCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 13)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy Exp Date', N'DatePicker', CAST(10 AS Numeric(5, 0)), N'DATE', CAST(0 AS Numeric(5, 0)), N'PolicyExpDate', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 10)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy ID', N'TextBox', CAST(20 AS Numeric(5, 0)), N'STRING', CAST(0 AS Numeric(5, 0)), N'PolicyNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 1)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy Product', N'ComboBox', CAST(2 AS Numeric(5, 0)), N'Policy_Product', CAST(0 AS Numeric(5, 0)), N'IndustryProductCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 7)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy Program', N'ComboBox', CAST(10 AS Numeric(5, 0)), N'Policy_Program', CAST(0 AS Numeric(5, 0)), N'ProgramCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 9)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy State', N'ComboBox', CAST(2 AS Numeric(5, 0)), N'Policy_State', CAST(0 AS Numeric(5, 0)), N'DomicileStateCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 8)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy StatusInd', N'TextBox', CAST(1 AS Numeric(5, 0)), N'STRING', CAST(0 AS Numeric(5, 0)), N'PolicyStatusInd', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 14)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy Term Eff Date', N'DatePicker', CAST(10 AS Numeric(5, 0)), N'DATE', CAST(0 AS Numeric(5, 0)), N'TermEffDate', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 11)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Policy Transaction Eff Date', N'DatePicker', CAST(10 AS Numeric(5, 0)), N'DATE', CAST(0 AS Numeric(5, 0)), N'TransactionExpDate', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 12)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Vehicle Body', N'ComboBox', CAST(20 AS Numeric(5, 0)), N'Policy_VehBody', CAST(3 AS Numeric(5, 0)), N'VehicleBodyStyleCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 34)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Vehicle Make', N'ComboBox', CAST(20 AS Numeric(5, 0)), N'Policy_VehMake', CAST(3 AS Numeric(5, 0)), N'VehicleMakeCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 32)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Vehicle Model', N'ComboBox', CAST(20 AS Numeric(5, 0)), N'Policy_VehModel', CAST(3 AS Numeric(5, 0)), N'VehicleModelCode', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 33)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Vehicle VIN', N'TextBox', CAST(17 AS Numeric(5, 0)), N'STRING', CAST(3 AS Numeric(5, 0)), N'VINNo', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 30)
INSERT [dbo].[SearchFields] ([SearchType], [FieldName], [FieldType], [FieldLength], [FieldEditCodeValues], [JoinID], [Column], [EffDate], [ExpDate], [HighOrderDispl]) VALUES (N'Policy', N'Vehicle Year', N'TextBox', CAST(4 AS Numeric(5, 0)), N'INTEGER', CAST(3 AS Numeric(5, 0)), N'VehicleYear', CAST(0x00009CB000000000 AS DateTime), CAST(0x0000D76F00000000 AS DateTime), 31)
