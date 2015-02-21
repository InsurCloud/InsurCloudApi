USE [pgmMaster]
GO
/****** Object:  Table [dbo].[ProgramCompany]    Script Date: 7/27/2014 4:25:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ProgramCompany](
	[ProgramCode] [nchar](10) NOT NULL,
	[ProgramName] [nvarchar](50) NOT NULL,
	[ProductType] [varchar](50) NOT NULL,
	[State] [nchar](10) NOT NULL,
	[EffDate] [smalldatetime] NOT NULL,
	[ExpDate] [smalldatetime] NOT NULL,
	[InsertDT] [smalldatetime] NULL,
 CONSTRAINT [PK_ProgramCompany] PRIMARY KEY CLUSTERED 
(
	[ProgramCode] ASC,
	[ProgramName] ASC,
	[ProductType] ASC,
	[State] ASC,
	[EffDate] ASC,
	[ExpDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'CIT', N'Property', N'LA        ', CAST(0x9A160000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'DP1', N'Property', N'LA        ', CAST(0x9B650000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'DP2', N'Property', N'LA        ', CAST(0x98A90000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'DP3', N'Property', N'LA        ', CAST(0x98A90000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'H20', N'Property', N'LA        ', CAST(0x98A90000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'H30', N'Property', N'LA        ', CAST(0x98A90000 AS SmallDateTime), CAST(0x9AC70000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'H30', N'Property', N'LA        ', CAST(0x9AC70000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'H3T', N'Property', N'LA        ', CAST(0x98A90000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'HOA', N'Property', N'TX        ', CAST(0x98100000 AS SmallDateTime), CAST(0x9B650000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'HOA', N'Property', N'TX        ', CAST(0x9B650000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'HOB', N'Property', N'TX        ', CAST(0x98100000 AS SmallDateTime), CAST(0x9B650000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'HOB', N'Property', N'TX        ', CAST(0x9B650000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'HOT', N'Property', N'TX        ', CAST(0x98100000 AS SmallDateTime), CAST(0x9B650000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'HOT', N'Property', N'TX        ', CAST(0x9B650000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'TD2', N'Property', N'TX        ', CAST(0x98100000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'TD3', N'Property', N'TX        ', CAST(0x98100000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'1         ', N'TDP', N'Property', N'TX        ', CAST(0x98100000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'6MO', N'Personal Auto', N'TX        ', CAST(0x971A0000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'AR6', N'Personal Auto', N'AR        ', CAST(0x971D0000 AS SmallDateTime), CAST(0x9A070000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'AR6', N'Personal Auto', N'AR        ', CAST(0x9A070000 AS SmallDateTime), CAST(0x9A9D0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'AR6', N'Personal Auto', N'AR        ', CAST(0x9A9D0000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x971D0000 AS SmallDateTime), CAST(0x98F90000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x98F90000 AS SmallDateTime), CAST(0x99270000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x99270000 AS SmallDateTime), CAST(0x99C10000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x99C10000 AS SmallDateTime), CAST(0x9A7F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x9A7F0000 AS SmallDateTime), CAST(0x9B1A0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x9B1A0000 AS SmallDateTime), CAST(0x9B280000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LA6', N'Personal Auto', N'LA        ', CAST(0x9B280000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LAC', N'Personal Auto', N'LA        ', CAST(0x9B110000 AS SmallDateTime), CAST(0x9B280000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'LAC', N'Personal Auto', N'LA        ', CAST(0x9B280000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'RIS', N'Personal Auto', N'TX        ', CAST(0x97B40000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x971D0000 AS SmallDateTime), CAST(0x97B40000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x97B40000 AS SmallDateTime), CAST(0x98F20000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x98F20000 AS SmallDateTime), CAST(0x999C0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x999C0000 AS SmallDateTime), CAST(0x9A710000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x9A710000 AS SmallDateTime), CAST(0x9A9D0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x9A9D0000 AS SmallDateTime), CAST(0x9B840000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TX6', N'Personal Auto', N'TX        ', CAST(0x9B840000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
INSERT [dbo].[ProgramCompany] ([ProgramCode], [ProgramName], [ProductType], [State], [EffDate], [ExpDate], [InsertDT]) VALUES (N'2         ', N'TXM', N'Personal Auto', N'TX        ', CAST(0x971D0000 AS SmallDateTime), CAST(0xD76F0000 AS SmallDateTime), CAST(0x9B8B02A2 AS SmallDateTime))
