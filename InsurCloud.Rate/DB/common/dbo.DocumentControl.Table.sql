USE [Common]
GO
/****** Object:  Table [dbo].[DocumentControl]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DocumentControl](
	[pgm] [varchar](3) NOT NULL,
	[DocName] [varchar](50) NULL,
	[DocType] [varchar](25) NOT NULL,
	[State] [char](2) NULL,
	[PlexType] [char](1) NULL,
	[Stock] [char](15) NULL,
	[CERTType] [varchar](50) NULL,
	[Stapling] [char](1) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Renewal Conversion Packet', N'RNL', N'AZ', N'D', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Renewal Quote', N'RENQ', N'AZ', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'DecPage', N'MC', N'AZ', N'S', N'REG            ', NULL, NULL)
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'DecPage', N'RDEC', N'AZ', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'DecPage', N'NDEC', N'AZ', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'DecPage', N'EDEC', N'AZ', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'IDCard', N'IDCR', N'AZ', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Cancellation', N'CXN', N'AZ', N'D', N'13PERF         ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Invoice', N'INV', N'AZ', N'S', N'13PERF         ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'NonRenewal', N'NRW', N'AZ', N'D', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Renewal Conversion Packet', N'A-RNL', N'AZ', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Reinstatement', N'REIN', N'AZ', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'202', N'Underwriting Letter', N'MIR', N'AZ', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'DecPage', N'MC', N'LA', N'S', N'REG            ', N'OTR', NULL)
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'DecPage', N'RDEC', N'AR', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'DecPage', N'NDEC', N'AR', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'DecPage', N'EDEC', N'AR', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'IDCard', N'IDCR', N'AR', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'Cancellation', N'CANCELLATION NOTICE', N'AR', N'S', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'203', N'Invoice', N'INVOICE', N'AR', N'S', N'1/3            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Renewal Conversion Packet', N'RNL', N'FL', N'D', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Renewal Quote', N'RENQ', N'FL', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'DecPage', N'MC', N'FL', N'S', N'REG            ', NULL, NULL)
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'DecPage', N'RDEC', N'FL', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'DecPage', N'NDEC', N'FL', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'DecPage', N'EDEC', N'FL', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'IDCard', N'IDCR', N'FL', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Cancellation', N'CXN', N'FL', N'D', N'13PERF         ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Invoice', N'INV', N'FL', N'S', N'13PERF         ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'NonRenewal', N'NRW', N'FL', N'D', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Renewal Conversion Packet', N'A-RNL', N'FL', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Reinstatement', N'REIN', N'FL', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'ThreeOptionLetter', N'3OL', N'FL', N'S', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Cancellation', N'CXU', N'FL', N'D', N'13PERF         ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'DecPage', N'NDEC', N'FL', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'209', N'Underwriting Letter', N'MIR', N'FL', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'DecPage', N'MC', N'LA', N'S', N'REG            ', NULL, NULL)
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'DecPage', N'RDEC', N'LA', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'DecPage', N'NDEC', N'LA', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'DecPage', N'EDEC', N'LA', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'IDCard', N'IDCR', N'LA', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'Cancellation', N'CXN', N'LA', N'D', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'Invoice', N'INV', N'LA', N'S', N'1/3            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'Renewal Quote', N'RENQ', N'LA', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'Renewal Conversion Packet', N'RNL', N'LA', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'Cancellation', N'CXU', N'LA', N'D', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'DecPage', N'RDEC', N'LA', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'DecPage', N'NDEC', N'LA', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'217', N'Reinstatement', N'REIN', N'LA', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'MailCover', N'MC', N'OK', N'S', N'REG            ', NULL, NULL)
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'DecPage', N'RDEC', N'OK', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'DecPage', N'NDEC', N'OK', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'DecPage', N'EDEC', N'OK', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'IDCard', N'IDCR', N'OK', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'Cancellation', N'CXN', N'OK', N'S', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'Invoice', N'INV', N'OK', N'S', N'13PERF         ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'Renewal Quote', N'RENQ', N'OK', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'Renewal Conversion Packet', N'RNL', N'OK', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'Cancellation', N'CXU', N'OK', N'S', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'235', N'Reinstatement', N'REIN', N'OK', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'DecPage', N'MC', N'LA', N'S', N'REG            ', N'OTR', NULL)
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'DecPage', N'RDEC', N'TX', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'DecPage', N'NDEC', N'TX', N'D', N'REG            ', N'OTR', N'1')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'DecPage', N'EDEC', N'TX', N'D', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'IDCard', N'IDCR', N'TX', N'S', N'REG            ', N'OTR', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'Cancellation', N'CANCELLATION NOTICE', N'TX', N'S', N'REG            ', N'CERT', N'0')
INSERT [dbo].[DocumentControl] ([pgm], [DocName], [DocType], [State], [PlexType], [Stock], [CERTType], [Stapling]) VALUES (N'242', N'Invoice', N'INVOICE', N'TX', N'S', N'1/3            ', N'OTR', N'0')
