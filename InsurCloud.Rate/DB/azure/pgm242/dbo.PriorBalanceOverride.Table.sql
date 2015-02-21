/****** Object:  Table [dbo].[PriorBalanceOverride]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PriorBalanceOverride](
	[PolicyID] [varchar](20) NOT NULL,
	[ExcludeBeforeDate] [datetime] NOT NULL,
	[ReasonDesc] [varchar](2000) NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_PriorBalanceOverride] PRIMARY KEY CLUSTERED 
(
	[PolicyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

GO
SET ANSI_PADDING OFF
GO
