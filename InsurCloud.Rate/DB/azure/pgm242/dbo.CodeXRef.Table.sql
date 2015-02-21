/****** Object:  Table [dbo].[CodeXRef]    Script Date: 7/26/2014 4:43:14 PM ******/
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
	[MappingCode1] [varchar](20) NOT NULL,
	[MappingCode2] [varchar](20) NULL,
	[MappingCode3] [varchar](20) NULL,
 CONSTRAINT [PK_CodeXRef] PRIMARY KEY CLUSTERED 
(
	[Source] ASC,
	[CodeType] ASC,
	[Code] ASC,
	[MappingCode1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON))

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'BRANCHCODE', N'005', N'TX', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'BI', N'BI', N'SplitLimit', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'CEQ', N'SPE', N'SplitLimit', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'COL', N'COL', N'Deductible1Amt', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'LLS', N'LLS', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'MED', N'MED', N'Limit1Amt', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'OTC', N'OTC', N'Deductible1Amt', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'PD', N'PD', N'Limit3Amt', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'PIP', N'PIP', N'Limit1Amt', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'REN', N'REN', N'SplitLimit', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'TOW', N'TOW', N'Limit3Amt', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'UBI', N'UUMBI', N'SplitLimit', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVERAGE', N'UPD', N'UUMPD', N'LimitWithDeductible', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'1', N'BI', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'10', N'CEQ', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'11', N'LLS', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'12', N'PIP', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'2', N'PD', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'3', N'UBI', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'4', N'UPD', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'5', N'COL', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'6', N'OTC', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'7', N'MED', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'8', N'REN', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'COVORDER', N'9', N'TOW', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'EFTPLAN', N'002', N'TAA', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'EFTPLAN', N'002', N'TX6', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'EFTPLAN', N'002', N'TXD', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'GENDER', N'F', N'Female', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'GENDER', N'M', N'Male', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'LICENSESTATUS', N'EXP', N'EXPIRED', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'LICENSESTATUS', N'ID', N'ID ONLY', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'LICENSESTATUS', N'PER', N'PERMIT', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'LICENSESTATUS', N'REV', N'REVOKED/CANCELLED', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'LICENSESTATUS', N'SUS', N'SUSPENDED', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'LICENSESTATUS', N'VAL', N'VALID', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'MARITALSTATUS', N'M', N'Married', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'MARITALSTATUS', N'S', N'Single', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'MARITALSTATUS', N'W', N'Widowed', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RATEPIF', N'TAA', N'TAA', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RATEPIF', N'TX6', N'TX6', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RATEPIF', N'TXD', N'TXD', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RELATIONSHIP', N'CHI', N'CHILD', N'Child', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RELATIONSHIP', N'OTR', N'OTHER', N'Other', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RELATIONSHIP', N'PAR', N'PARENT', N'Parent', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RELATIONSHIP', N'SEL', N'SELF', N'Self', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RELATIONSHIP', N'SIB', N'SIBLING', N'Sibling', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RELATIONSHIP', N'SPO', N'SPOUSE', N'Spouse', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'20 P Day/600 Max', N'30/600', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'20 Per Day/600 Per Occ', N'30/600', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'20/600', N'30/600', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'30 Per Day/900 Per Occ', N'30/900', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'30/900', N'30/900', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'40 Per Day/1200 Per Occ', N'30/1200', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'RENTALCOVLIMIT', N'40/1200', N'30/1200', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'STATUS', N'D', N'ACTIVE', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'STATUS', N'P', N'PERMITTED', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'STATUS', N'U', N'NHH', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'STATUS', N'X', N'EXCLUDED', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'UseTransferFactorYN', N'Classic', N'False', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'VIOLSOURCECODE', N'C', N'CLAIMS', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'VIOLSOURCECODE', N'M', N'MVR', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'VIOLSOURCECODE', N'N', N'INPUT', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PAS', N'VIOLSOURCECODE', N'U', N'CLUE', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'LIMITS', N'10', N'2', N'25/50', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'LIMITS', N'2', N'1', N'20/40', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'LIMITS', N'20', N'3', N'50/100', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'LIMITS', N'30', N'4', N'100/300 or 100+ CSL', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'MARITALSTATUS', N'A', N'Single', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'MARITALSTATUS', N'D', N'Single', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'MARITALSTATUS', N'M', N'Married', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'MARITALSTATUS', N'S', N'Single', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'MARITALSTATUS', N'W', N'Single', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PAYPLAN', N'200', N'100', N'PayPlan Code', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PIF', N'TAA', N'PolicyFactor', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PIF', N'TX6', N'PolicyFactor', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PIF', N'TXD', N'Policy Factor', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'10000.00', N'20000.00', N'0')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'100000.00', N'300000.00', N'3')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'100060.00', N'300060.00', N'3')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'25000.00', N'50000.00', N'1')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'25060.00', N'50060.00', N'1')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'30000.00', N'60000.00', N'1')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'30060.00', N'60060.00', N'2')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'50000.00', N'100000.00', N'2')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'PRIOR', N'LIMITS', N'50060.00', N'100060.00', N'2')
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'Program', N'Classic', N'TAA', N'AUTO', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'Program', N'Direct', N'TXD', N'AUTO', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'Program', N'Summit', N'TX6', N'AUTO', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'Program', N'TAA', N'Classic', N'AUTO', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'Program', N'TX6', N'Summit', N'AUTO', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'Program', N'TXD', N'Direct', N'AUTO', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'BRO', N'SIBLING', N'Sibling', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'CHI', N'CHILD', N'Child', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'NON', N'OTHER', N'Other', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'OTR', N'OTHER', N'Other', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'PAR', N'PARENT', N'Parent', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'PRT', N'SPOUSE', N'Spouse', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'SEL', N'SELF', N'Self', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'SIB', N'SIBLING', N'Sibling', NULL)
GO
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'SIS', N'SIBLING', N'Sibling', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RELATIONSHIP', N'SPO', N'SPOUSE', N'Spouse', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RESIDENCE', N'H', N'OH', N'Own Home', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RESIDENCE', N'LWP', N'LP', N'Live with Parents', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RESIDENCE', N'MH', N'OM', N'Own Mobile Home', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RESIDENCE', N'O', N'OT', N'Other', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'RESIDENCE', N'R', N'RT', N'Rent', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'VEHICLEFACTOR', N'BUS', N'BUS_USE', N'Business Use', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRATE', N'VEHICLEFACTOR', N'WED', N'ETCH', N'Business Use', NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'11230', N'11231', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'11395', N'11396', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'30110', N'30111', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'30150', N'30151', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'31250', N'31251', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'31255', N'31256', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'36400', N'36401', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'55557', N'55559', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASRENEW', N'VIOLUPDATE', N'59999', N'59998', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASREPORTS', N'APA', N'Classic', N'11/30/2008', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASREPORTS', N'APA', N'Direct', N'10/1/2012', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PASREPORTS', N'APA', N'Summit', N'11/30/2008', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PROGRAM', N'CODE', N'Classic', N'TAA', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PROGRAM', N'CODE', N'Direct', N'TXD', NULL, NULL)
INSERT [dbo].[CodeXRef] ([Source], [CodeType], [Code], [MappingCode1], [MappingCode2], [MappingCode3]) VALUES (N'PROGRAM', N'CODE', N'Summit', N'TX6', NULL, NULL)
