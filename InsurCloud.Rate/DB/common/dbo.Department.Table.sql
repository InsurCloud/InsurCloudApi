USE [Common]
GO
/****** Object:  Table [dbo].[Department]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Department](
	[CompanyCode] [char](3) NOT NULL,
	[DepartmentCode] [char](3) NOT NULL,
	[DepartmentDesc] [varchar](90) NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
 CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED 
(
	[CompanyCode] ASC,
	[DepartmentCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[Department] ([CompanyCode], [DepartmentCode], [DepartmentDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'HS ', N'002', N'CLAIMS', CAST(0x00009C2900E13264 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[Department] ([CompanyCode], [DepartmentCode], [DepartmentDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'IF ', N'002', N'CLAIMS', CAST(0x0000972001231788 AS DateTime), N'TRACYLANCASTER                ', NULL, NULL)
INSERT [dbo].[Department] ([CompanyCode], [DepartmentCode], [DepartmentDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'IF ', N'UW ', N'Underwriting', CAST(0x0000971400E896F8 AS DateTime), N'MARKWDEPPERSCHMIDT            ', NULL, NULL)
INSERT [dbo].[Department] ([CompanyCode], [DepartmentCode], [DepartmentDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'NT ', N'002', N'CLAIMS', CAST(0x0000A26C00998D60 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[Department] ([CompanyCode], [DepartmentCode], [DepartmentDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'NT ', N'UW ', N'Underwriting', CAST(0x0000A26C009996C0 AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
ALTER TABLE [dbo].[Department]  WITH CHECK ADD  CONSTRAINT [FK_Department_Company] FOREIGN KEY([CompanyCode])
REFERENCES [dbo].[Company] ([CompanyCode])
GO
ALTER TABLE [dbo].[Department] CHECK CONSTRAINT [FK_Department_Company]
GO
