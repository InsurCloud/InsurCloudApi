USE [pgm242]
GO
/****** Object:  Table [dbo].[AgentDefaults]    Script Date: 7/26/2014 4:28:10 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AgentDefaults](
	[AgentID] [varchar](50) NOT NULL,
	[BI] [nchar](10) NULL,
	[PD] [nchar](10) NULL,
	[MED] [nchar](10) NULL,
	[COM] [nchar](10) NULL,
	[COL] [nchar](10) NULL,
	[OTC] [nchar](10) NULL,
	[REN] [nchar](10) NULL,
	[TOW] [nchar](10) NULL,
	[SPE] [nchar](10) NULL,
	[UUMBI] [nchar](10) NULL,
	[UUMPD] [nchar](10) NULL,
	[UMBI] [nchar](10) NULL,
	[UMPD] [nchar](10) NULL,
	[UIMBI] [nchar](10) NULL,
	[UIMPD] [nchar](10) NULL,
	[ADI] [nchar](10) NULL,
	[PayPlan] [nchar](10) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
