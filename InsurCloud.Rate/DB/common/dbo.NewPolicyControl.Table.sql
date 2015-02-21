USE [Common]
GO
/****** Object:  Table [dbo].[NewPolicyControl]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[NewPolicyControl](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedUser] [varchar](50) NOT NULL,
	[CompanyCode] [varchar](3) NOT NULL,
	[ProgramCode] [varchar](3) NOT NULL,
	[PolicyID] [varchar](15) NOT NULL,
	[CurrentActualEffDate] [datetime] NULL,
	[PCCurrentActualEffDate] [datetime] NULL,
	[TermEffDate] [datetime] NULL,
	[MaxTermEffDate] [datetime] NULL,
	[PolicyTrans] [numeric](10, 0) NULL,
	[MaxPolicyTrans] [numeric](10, 0) NULL,
	[PolicyExpDate] [datetime] NULL,
	[MaxPolicyExpDate] [datetime] NULL,
	[IsPA] [bit] NULL,
	[IsHOME] [bit] NULL,
	[BillingStatusInd] [varchar](5) NULL,
	[LastTermEffDate] [datetime] NULL,
	[CurrentTermEff] [datetime] NULL,
	[LastPolicyTrans] [numeric](10, 0) NULL,
	[CurrentTran] [numeric](10, 0) NULL,
	[ManualCancelEffDate] [datetime] NULL,
	[CancellationDate] [datetime] NULL,
	[Success] [varchar](800) NULL,
 CONSTRAINT [PK_CreatedDate] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
