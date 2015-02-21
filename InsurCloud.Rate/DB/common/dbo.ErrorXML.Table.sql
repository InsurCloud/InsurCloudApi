USE [Common]
GO
/****** Object:  Table [dbo].[ErrorXML]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ErrorXML](
	[ErrorID] [int] IDENTITY(1,1) NOT NULL,
	[ErrorXML] [xml] NOT NULL,
	[SourceSystem] [varchar](50) NOT NULL,
	[ReferenceType] [varchar](50) NULL,
	[ReferenceID] [varchar](50) NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_Endorsement] PRIMARY KEY CLUSTERED 
(
	[ErrorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [ix_ErrorXML_c+i]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [ix_ErrorXML_c+i] ON [dbo].[ErrorXML]
(
	[SourceSystem] ASC,
	[ReferenceType] ASC,
	[SystemTS] ASC
)
INCLUDE ( 	[ErrorID],
	[ReferenceID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [ix_ErrorXML_SystemTS]    Script Date: 7/27/2014 2:06:56 PM ******/
CREATE NONCLUSTERED INDEX [ix_ErrorXML_SystemTS] ON [dbo].[ErrorXML]
(
	[SystemTS] ASC
)
INCLUDE ( 	[ErrorID],
	[ErrorXML],
	[SourceSystem],
	[ReferenceType],
	[ReferenceID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
