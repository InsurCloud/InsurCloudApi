USE [Common]
GO
/****** Object:  Table [dbo].[BatchError]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BatchError](
	[ErrorID] [numeric](18, 0) NOT NULL,
	[ActivityID] [numeric](18, 0) NULL,
	[Process] [varchar](100) NULL,
	[RecordID] [varchar](50) NULL,
	[Msg] [varchar](3000) NULL,
	[Status] [varchar](50) NULL,
	[UserID] [varchar](50) NULL,
	[SystemTS] [datetime] NULL,
 CONSTRAINT [PK_BatchError] PRIMARY KEY CLUSTERED 
(
	[ErrorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
