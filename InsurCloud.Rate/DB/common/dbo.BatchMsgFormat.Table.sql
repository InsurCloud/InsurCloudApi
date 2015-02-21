USE [Common]
GO
/****** Object:  Table [dbo].[BatchMsgFormat]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BatchMsgFormat](
	[MsgType] [varchar](50) NOT NULL,
	[MsgSubType] [varchar](50) NULL,
	[MsgEffDate] [varchar](50) NULL,
	[PolicyID] [nvarchar](50) NULL,
	[TermEffDate] [nvarchar](50) NULL,
	[PolicyTransactionNum] [nvarchar](50) NULL,
	[Product] [nvarchar](50) NULL,
	[State] [nvarchar](50) NULL,
	[PolicyXML] [nvarchar](50) NULL,
	[Param1] [nvarchar](50) NULL,
	[Param2] [nvarchar](50) NULL,
	[Param3] [nvarchar](50) NULL,
	[Confirmation] [nvarchar](50) NULL,
	[ProcessedDate] [nvarchar](50) NULL,
	[ArchiveReason] [nvarchar](50) NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_BatchMsgFormat] PRIMARY KEY CLUSTERED 
(
	[MsgType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
