/****** Object:  Table [dbo].[RateDateOverride]    Script Date: 7/26/2014 4:39:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RateDateOverride](
	[Program] [varchar](50) NOT NULL,
	[ProgramCode] [varchar](50) NULL,
	[AppliesToCode] [varchar](1) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ExpDate] [datetime] NOT NULL,
	[RateVersionDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RateDateOverride] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[AppliesToCode] ASC,
	[EffDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
