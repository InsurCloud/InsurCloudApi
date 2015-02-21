USE [Common]
GO
/****** Object:  Table [dbo].[InstallmentsWithoutBatchMsg]    Script Date: 7/27/2014 2:06:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[InstallmentsWithoutBatchMsg](
	[ID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[ProgramCode] [varchar](3) NOT NULL,
	[PolicyNo] [varchar](15) NOT NULL,
	[InstallmentNum] [varchar](15) NOT NULL,
	[DueDate] [datetime] NOT NULL,
	[Balance] [numeric](9, 2) NOT NULL,
	[WorkedBy] [varchar](50) NULL,
	[WorkedOn] [datetime] NULL,
	[UserID] [varchar](50) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_InstallmentsWithoutBatchMsg] PRIMARY KEY CLUSTERED 
(
	[PolicyNo] ASC,
	[InstallmentNum] ASC,
	[SystemTS] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
