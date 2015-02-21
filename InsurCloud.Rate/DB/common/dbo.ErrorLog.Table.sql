USE [Common]
GO
/****** Object:  Table [dbo].[ErrorLog]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorLog](
	[ErrorID] [int] IDENTITY(1,1) NOT NULL,
	[Source] [text] NULL,
	[Message] [text] NULL,
	[StackTrace] [text] NULL,
	[OffendingURL] [text] NULL,
	[TSCreate] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
