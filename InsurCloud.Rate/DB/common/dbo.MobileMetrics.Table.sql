USE [Common]
GO
/****** Object:  Table [dbo].[MobileMetrics]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[MobileMetrics](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](max) NOT NULL,
	[DeviceType] [nvarchar](max) NOT NULL,
	[OSVersion] [varchar](50) NOT NULL,
	[AppVersion] [varchar](50) NULL,
	[Method] [nvarchar](max) NOT NULL,
	[StartTS] [datetime] NOT NULL,
	[EndTS] [datetime] NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_MobileMetrics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
