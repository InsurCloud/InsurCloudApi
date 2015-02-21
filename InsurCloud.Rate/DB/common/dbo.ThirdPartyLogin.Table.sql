USE [Common]
GO
/****** Object:  Table [dbo].[ThirdPartyLogin]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ThirdPartyLogin](
	[CompanyName] [varchar](50) NOT NULL,
	[LoginType] [varchar](50) NOT NULL,
	[LoginName] [varchar](max) NULL,
	[Password] [varchar](max) NOT NULL,
	[UserId] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
