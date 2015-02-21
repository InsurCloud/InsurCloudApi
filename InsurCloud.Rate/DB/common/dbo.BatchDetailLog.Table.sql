USE [Common]
GO
/****** Object:  Table [dbo].[BatchDetailLog]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BatchDetailLog](
	[ActivityID] [numeric](18, 0) NOT NULL,
	[Process] [varchar](100) NULL,
	[Product] [numeric](18, 0) NULL,
	[State] [numeric](18, 0) NULL,
	[RecordID] [varchar](50) NULL,
	[Msg] [varchar](3000) NULL,
	[UserID] [varchar](50) NULL,
	[SystemTS] [datetime] NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
