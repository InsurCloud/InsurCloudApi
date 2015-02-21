USE [Common]
GO
/****** Object:  Table [dbo].[SessionOld]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SessionOld](
	[SessionID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](50) NOT NULL,
	[PolicyID] [varchar](25) NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[Producer] [varchar](50) NOT NULL,
	[AgencyID] [varchar](50) NOT NULL,
	[isUWCorrection] [bit] NOT NULL,
	[isAdmin] [bit] NOT NULL,
	[EndorsementID] [int] NULL,
	[Program] [varchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
