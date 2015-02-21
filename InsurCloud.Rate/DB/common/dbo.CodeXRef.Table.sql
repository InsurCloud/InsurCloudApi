USE [Common]
GO
/****** Object:  Table [dbo].[CodeXRef]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CodeXRef](
	[Source] [varchar](20) NOT NULL,
	[CodeType] [varchar](20) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[MappingCode1] [varchar](50) NOT NULL,
	[MappingCode2] [varchar](50) NULL,
	[MappingCode3] [varchar](50) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
