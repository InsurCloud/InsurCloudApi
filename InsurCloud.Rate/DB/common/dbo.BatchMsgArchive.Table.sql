USE [Common]
GO
/****** Object:  Table [dbo].[BatchMsgArchive]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[BatchMsgArchive](
	[MsgID] [numeric](18, 0) NOT NULL,
	[MsgEffDate] [datetime] NOT NULL,
	[MsgType] [varchar](50) NOT NULL,
	[MsgSubType] [varchar](50) NOT NULL,
	[PolicyID] [nvarchar](50) NULL,
	[TermEffDate] [datetime] NULL,
	[PolicyTransactionNum] [numeric](2, 0) NULL,
	[Product] [numeric](18, 0) NULL,
	[State] [numeric](18, 0) NULL,
	[PolicyXML] [xml] NULL,
	[Param1] [nvarchar](50) NULL,
	[Param2] [nvarchar](50) NULL,
	[Param3] [nvarchar](50) NULL,
	[ParamXML] [xml] NULL,
	[Confirmation] [nvarchar](250) NULL,
	[ProcessedDate] [datetime] NULL,
	[ArchiveReason] [varchar](100) NULL,
	[UserID] [varchar](25) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_BatchMsgArchive] PRIMARY KEY CLUSTERED 
(
	[MsgID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_BatchMsg_1]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_BatchMsg_1] ON [dbo].[BatchMsgArchive]
(
	[MsgType] ASC,
	[MsgSubType] ASC,
	[MsgEffDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
