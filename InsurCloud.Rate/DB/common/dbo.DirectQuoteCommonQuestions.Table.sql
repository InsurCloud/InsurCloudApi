USE [Common]
GO
/****** Object:  Table [dbo].[DirectQuoteCommonQuestions]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DirectQuoteCommonQuestions](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[QuestionNumber] [smallint] NOT NULL,
	[Question] [nvarchar](1000) NOT NULL,
	[Answer] [nvarchar](1000) NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
