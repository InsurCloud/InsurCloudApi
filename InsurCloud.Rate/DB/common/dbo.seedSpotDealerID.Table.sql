USE [Common]
GO
/****** Object:  Table [dbo].[seedSpotDealerID]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[seedSpotDealerID](
	[SpotAgentID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[SystemTS] [datetime] NULL
) ON [PRIMARY]

GO
