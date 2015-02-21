USE [Common]
GO
/****** Object:  Table [dbo].[DriverIncidentCategory]    Script Date: 7/27/2014 2:29:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DriverIncidentCategory](
	[DriverIncidentCategoryCode] [char](3) NOT NULL,
	[DriverIncidentCategoryDesc] [varchar](90) NULL,
	[AddedDateT] [datetime] NULL,
	[AddedUserCode] [char](30) NULL,
	[LastUpdatedDateT] [datetime] NULL,
	[LastUpdatedUserCode] [char](30) NULL,
 CONSTRAINT [PK_DriverIncidentCategory] PRIMARY KEY CLUSTERED 
(
	[DriverIncidentCategoryCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'AFA', N'At Fault Accident', CAST(0x0000974500896700 AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'CRD', N'Careless/Reckless Driving', CAST(0x00009AF1010B0CD8 AS DateTime), N'MINDYARVISU                   ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'DWI', N'DWI/DUI', CAST(0x00009745008936F4 AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'MAJ', N'Major', CAST(0x0000974500894F90 AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'MIN', N'Minor', CAST(0x0000974500894888 AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'NAF', N'Not At Fault Accident', CAST(0x0000974500895B48 AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'OT1', N'OTC < $1000', CAST(0x000097450089844C AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'OTC', N'OTC >= $1000', CAST(0x0000974500897768 AS DateTime), N'JHOUSE                        ', CAST(0x0000974500899130 AS DateTime), N'JHOUSE                        ')
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'S22', N'SR-22', CAST(0x00009AF1010B21F0 AS DateTime), N'MINDYARVISU                   ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'SPD', N'Speeding', CAST(0x0000974500894054 AS DateTime), N'JHOUSE                        ', NULL, NULL)
INSERT [dbo].[DriverIncidentCategory] ([DriverIncidentCategoryCode], [DriverIncidentCategoryDesc], [AddedDateT], [AddedUserCode], [LastUpdatedDateT], [LastUpdatedUserCode]) VALUES (N'UDR', N'Unverifiable Driving Record', CAST(0x00009AF101186BE4 AS DateTime), N'MINDYARVISU                   ', NULL, NULL)
