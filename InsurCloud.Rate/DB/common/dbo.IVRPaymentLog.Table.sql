USE [Common]
GO
/****** Object:  Table [dbo].[IVRPaymentLog]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[IVRPaymentLog](
	[PolicyId] [varchar](50) NOT NULL,
	[PaymentType] [varchar](50) NOT NULL,
	[Result] [varchar](50) NOT NULL,
	[Description] [varchar](max) NULL,
	[PaymentId] [numeric](18, 0) NULL,
	[PaymentLoc] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[IVRPaymentLog] ADD  CONSTRAINT [DF_IVRPaymentLog_SystemTS]  DEFAULT (getdate()) FOR [SystemTS]
GO
