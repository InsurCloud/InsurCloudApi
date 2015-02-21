USE [Common]
GO
/****** Object:  Table [dbo].[AccountingReportParameters]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AccountingReportParameters](
	[ReportName] [varchar](50) NOT NULL,
	[ParameterName] [varchar](50) NOT NULL,
	[Description] [varchar](150) NOT NULL,
	[ParameterType] [varchar](50) NOT NULL,
	[SQLDeclaration] [varchar](50) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [date] NOT NULL,
 CONSTRAINT [PK_AccountingReportParameters] PRIMARY KEY CLUSTERED 
(
	[ReportName] ASC,
	[ParameterName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[AccountingReportParameters] ([ReportName], [ParameterName], [Description], [ParameterType], [SQLDeclaration], [DisplayOrder], [UserID], [SystemTS]) VALUES (N'DirectPayCancelESign', N'FromDate', N'From Date:', N'Date', N'@FromDate', 1, N'shaun.herschbach', CAST(0xB0360B00 AS Date))
INSERT [dbo].[AccountingReportParameters] ([ReportName], [ParameterName], [Description], [ParameterType], [SQLDeclaration], [DisplayOrder], [UserID], [SystemTS]) VALUES (N'DirectPayCancelESign', N'ToDate', N'To Date:', N'Date', N'@ToDate', 2, N'shaun.herschbach', CAST(0xB0360B00 AS Date))
INSERT [dbo].[AccountingReportParameters] ([ReportName], [ParameterName], [Description], [ParameterType], [SQLDeclaration], [DisplayOrder], [UserID], [SystemTS]) VALUES (N'DirectPaySuspend', N'FromDate', N'From Date:', N'Date', N'@FromDate', 1, N'shaun.herschbach', CAST(0xB0360B00 AS Date))
INSERT [dbo].[AccountingReportParameters] ([ReportName], [ParameterName], [Description], [ParameterType], [SQLDeclaration], [DisplayOrder], [UserID], [SystemTS]) VALUES (N'DirectPaySuspend', N'ToDate', N'To Date:', N'Date', N'@ToDate', 2, N'shaun.herschbach', CAST(0xB0360B00 AS Date))
INSERT [dbo].[AccountingReportParameters] ([ReportName], [ParameterName], [Description], [ParameterType], [SQLDeclaration], [DisplayOrder], [UserID], [SystemTS]) VALUES (N'PolicyWithNoMoneyPosted', N'FromDate', N'From Date:', N'Date', N'@FromDate', 1, N'shaun.herschbach', CAST(0xB0360B00 AS Date))
INSERT [dbo].[AccountingReportParameters] ([ReportName], [ParameterName], [Description], [ParameterType], [SQLDeclaration], [DisplayOrder], [UserID], [SystemTS]) VALUES (N'PolicyWithNoMoneyPosted', N'ToDate', N'To Date:', N'Date', N'@ToDate', 2, N'shaun.herschbach', CAST(0xB0360B00 AS Date))
