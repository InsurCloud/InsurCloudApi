USE [Common]
GO
/****** Object:  Table [dbo].[TempLogging]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TempLogging](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LogDesc] [varchar](max) NOT NULL,
	[ReferenceID] [varchar](max) NULL,
	[UserID] [varchar](max) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
