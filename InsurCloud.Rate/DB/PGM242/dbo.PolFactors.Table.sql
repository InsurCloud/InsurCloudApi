USE [pgm242]
GO
/****** Object:  Table [dbo].[PolFactors]    Script Date: 7/26/2014 4:28:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PolFactors](
	[PolicyFactorID] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
	[CompanyCode] [char](3) NOT NULL,
	[ProgramCode] [char](3) NOT NULL,
	[PolicyID] [varchar](15) NOT NULL,
	[TermEffDate] [datetime] NOT NULL,
	[PolicyTransactionNum] [numeric](2, 0) NOT NULL,
	[FactorType] [varchar](50) NOT NULL,
	[FactorCode] [varchar](50) NOT NULL,
	[UnitNumber] [numeric](2, 0) NOT NULL,
	[SystemTS] [datetime] NOT NULL,
 CONSTRAINT [PK_PolicyFactors] PRIMARY KEY CLUSTERED 
(
	[PolicyFactorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
