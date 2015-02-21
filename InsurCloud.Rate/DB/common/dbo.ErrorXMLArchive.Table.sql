USE [Common]
GO
/****** Object:  Table [dbo].[ErrorXMLArchive]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ErrorXMLArchive](
	[ErrorID] [int] NOT NULL,
	[ErrorXML] [xml] NOT NULL,
	[SourceSystem] [varchar](50) NOT NULL,
	[ReferenceType] [varchar](50) NULL,
	[ReferenceID] [varchar](50) NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_ErrorXMLArchive] PRIMARY KEY CLUSTERED 
(
	[ErrorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
