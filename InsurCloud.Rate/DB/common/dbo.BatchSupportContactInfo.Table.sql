USE [Common]
GO
/****** Object:  Table [dbo].[BatchSupportContactInfo]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BatchSupportContactInfo](
	[OwnerName] [varchar](100) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[SecondaryEmail] [varchar](100) NULL,
	[PhoneNum] [varchar](100) NULL,
	[TextYN] [bit] NOT NULL,
	[TextEmail] [varchar](100) NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[BatchSupportContactInfo] ([OwnerName], [Email], [SecondaryEmail], [PhoneNum], [TextYN], [TextEmail], [UserID], [SystemTS]) VALUES (N'batch.sup', N'batch.support@imperialfire.com', NULL, NULL, 0, NULL, N'kevin.burger', CAST(0x0000A006010B9BAE AS DateTime))
INSERT [dbo].[BatchSupportContactInfo] ([OwnerName], [Email], [SecondaryEmail], [PhoneNum], [TextYN], [TextEmail], [UserID], [SystemTS]) VALUES (N'batch.ops', N'batch.operators@imperialfire.com', NULL, NULL, 0, NULL, N'SH3443', CAST(0x00009E4B0125726C AS DateTime))
