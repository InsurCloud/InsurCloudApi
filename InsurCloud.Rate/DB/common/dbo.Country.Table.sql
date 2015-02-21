USE [Common]
GO
/****** Object:  Table [dbo].[Country]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Country](
	[CountryCode] [char](3) NOT NULL,
	[CountryName] [varchar](90) NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
 CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED 
(
	[CountryCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[Country] ([CountryCode], [CountryName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'CAN', N'Canada', NULL, NULL, NULL, NULL)
INSERT [dbo].[Country] ([CountryCode], [CountryName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'FOR', N'Foreign', CAST(0x00009AF10111B5EC AS DateTime), N'MINDYARVISU                   ', NULL, NULL)
INSERT [dbo].[Country] ([CountryCode], [CountryName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'INT', N'International', CAST(0x00009AF10111AC8C AS DateTime), N'MINDYARVISU                   ', NULL, NULL)
INSERT [dbo].[Country] ([CountryCode], [CountryName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'MEX', N'Mexico', CAST(0x000097AA00F2826C AS DateTime), N'BRENDABORDELON                ', NULL, NULL)
INSERT [dbo].[Country] ([CountryCode], [CountryName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'UK ', N'United Kingdom', NULL, NULL, NULL, NULL)
INSERT [dbo].[Country] ([CountryCode], [CountryName], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'USA', N'United States of America', NULL, NULL, NULL, NULL)
