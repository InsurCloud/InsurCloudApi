USE [Common]
GO
/****** Object:  Table [dbo].[TableJoins]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[TableJoins](
	[JoinID] [numeric](5, 0) IDENTITY(0,1) NOT NULL,
	[JoinTable] [varchar](max) NOT NULL,
	[JoinOn] [varchar](max) NOT NULL,
	[Alias] [varchar](50) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
 CONSTRAINT [PK_TableJoins] PRIMARY KEY CLUSTERED 
(
	[JoinID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
