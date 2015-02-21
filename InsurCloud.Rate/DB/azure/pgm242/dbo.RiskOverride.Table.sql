/****** Object:  Table [dbo].[RiskOverride]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[RiskOverride](
	[OverrideID] [int] IDENTITY(1,1) NOT NULL,
	[PolicyNo] [varchar](50) NULL,
	[QuoteID] [varchar](50) NULL,
	[TermEffDate] [datetime] NOT NULL,
	[RiskCode] [varchar](50) NOT NULL,
	[RiskDescription] [varchar](max) NOT NULL,
	[AddedUserID] [varchar](50) NOT NULL,
	[AddedTS] [datetime] NOT NULL,
	[AddedComments] [varchar](max) NOT NULL,
	[DeletedFlag] [bit] NOT NULL,
	[DeletedUserID] [varchar](50) NULL,
	[DeletedTS] [datetime] NULL,
	[DeletedComments] [varchar](max) NULL,
 CONSTRAINT [PK_RiskOverride] PRIMARY KEY CLUSTERED 
(
	[OverrideID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
SET ANSI_PADDING OFF
GO
