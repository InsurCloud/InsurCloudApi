USE [Common]
GO
/****** Object:  Table [dbo].[WeatherLookup]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WeatherLookup](
	[ZipCode] [numeric](5, 0) NOT NULL,
	[LastLookupDate] [datetime] NOT NULL,
	[LookupResult] [xml] NOT NULL,
 CONSTRAINT [PK_WeatherLookup] PRIMARY KEY CLUSTERED 
(
	[ZipCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Index [IX_WeatherLookup]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [IX_WeatherLookup] ON [dbo].[WeatherLookup]
(
	[ZipCode] ASC,
	[LastLookupDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
