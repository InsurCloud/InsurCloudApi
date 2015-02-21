USE [pgm242]
GO
/****** Object:  Table [dbo].[AuditLog]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AuditLog](
	[AuditID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[AuditStatus] [varchar](50) NULL,
	[FollowUpBy] [varchar](50) NULL,
	[FollowUpDate] [datetime] NULL,
	[FollowUpSetBy] [varchar](50) NULL,
	[FollowUpNote] [varchar](max) NULL,
	[ResolvedBy] [varchar](50) NULL,
	[ResolvedDate] [datetime] NULL,
	[ResolvedNote] [varchar](max) NULL,
	[PolicyID] [varchar](50) NULL,
	[TransNum] [numeric](2, 0) NULL,
	[SourceSystem] [varchar](50) NULL,
	[SourceID] [varchar](50) NULL,
	[CreatedBy] [varchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ActionType] [nvarchar](50) NULL,
	[ActionOffense] [varchar](max) NULL,
	[ActionDetails] [varchar](max) NULL,
	[ParentID] [numeric](18, 0) NULL,
	[TermEffDate] [datetime] NULL,
	[PrintTS] [datetime] NULL,
	[ImageID] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
