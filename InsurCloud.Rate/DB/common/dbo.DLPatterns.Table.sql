USE [Common]
GO
/****** Object:  Table [dbo].[DLPatterns]    Script Date: 7/29/2014 2:57:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DLPatterns](
	[State] [varchar](2) NOT NULL,
	[Pattern] [varchar](200) NOT NULL,
	[Comment] [varchar](500) NULL,
 CONSTRAINT [PK_DLPatterns] PRIMARY KEY CLUSTERED 
(
	[State] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'AK', N'^\d{1,7}$', N'1 to 7 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'AL', N'^\d{7}$', N'7 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'AR', N'^\d{8,9}$', N'9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'AZ', N'^([A-Z]\d{8}|[A-Z]{2}\d{3,6}|\d{9})$', N'1 Alpha, 8 Numeric or 2 Alpha, 3-6 Numeric or 9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'CA', N'^[A-Z]\d{7}$', N'1 Alpha, 7 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'CO', N'^\d{9}$', N'9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'CT', N'^\d{9}$', N'9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'DC', N'^(\d{7}|\d{9})$', N'7 Numeric or 9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'DE', N'^\d{1,7}$', N'1-7 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'FL', N'^[A-Z]\d{12}$', N'1 Alpha, 12 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'FN', N'^[A-Za-z0-9]*$', N'No restriction')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'GA', N'^\d{7,9}$', N'7-9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'HI', N'^(\d{9}|[H]\d{8})$', N' 9 Numeric or "H", 8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'IA', N'^(\d{3}[A-Z]{2}\d{4}|\d{9})$', N'3 Numeric, 2 Alpha, 4 Numeric or 9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'ID', N'^([A-Z]{2}\d{6}[A-Z]|\d{9})$', N' 2 Alpha, 6 Numeric, 1 Alpha or 9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'IL', N'^[A-Z]\d{11}$', N'1 Alpha, 11 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'IN', N'^\d{9,10}$', N'9-10 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'IT', N'^[A-Za-z0-9]*$', N'No restriction')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'KS', N'^(\d{9}|[A-Z]\d{8})$', N'9 Numeric or 1 Alpha, 8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'KY', N'^(\d{9}|[A-Z]\d{8})$', N'9 Numeric or 1 Alpha, 8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'LA', N'^\d{9}$', N'9 Numeric (If shorter than 9 digits, add 0s to the front of the number to make 9 digits)')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MA', N'^([A-Z]\d{8}|\d{9})$', N'1 Alpha, 8 Numeric or 9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MD', N'^[A-Z]\d{12}$', N'1 Alpha, 12 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'ME', N'^\d{7}[X]?$', N'7 Numeric or (under 21) 7 Numeric followed by an "X"')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MI', N'^[A-Z]\d{12}$', N'1 Alpha, 12 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MN', N'^[A-Z]\d{12}$', N'1 Alpha, 12 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MO', N'^(\d{9}|[A-Z]\d{5,9})$', N'9 Numeric or 1 Alpha, 5-9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MS', N'^\d{9}$', N'9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'MT', N'^(\d{9}|[A-Z]\d[\dA-Z]\d{2}[A-Z]{2}\1|\d{13})$', N'9 Numeric or 1 Alpha, 1 Numeric, 1 Alphanumeric, 2 Numeric, 2 Alpha, 1 Numberic or 13 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NC', N'^\d{1,8}$', N'1-8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'ND', N'^(\d{9}|[A-Z]{3}\d{6})$', N'9 Numeric or 3 Alpha, 6 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NE', N'^(A|B|C|E|G|H|V)\d{3,8}$', N'1 Alpha (A,B,C,E,G,H, or V), 3-8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NH', N'^\d{2}[A-Z]{3}\d{5}$', N'2 Numeric, 3 Alpha, 5 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NJ', N'^[A-Z]\d{14}$', N'1 Alpha, 14 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NM', N'^\d{9}$', N'9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NV', N'^(d{9}|\d{10}|\d{12})$', N'9 Numeric or 12 Numeric, or 10 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'NY', N'^(\d{9}|[A-Z]\d{19})$', N'9 Numeric or 1 Alpha, 18 Numeric (No longer issued)')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'OH', N'^(\d{9}|[A-Z]{2}\d{6})$', N'9 Numeric or 2 Alpha, 6 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'OK', N'^([A-Za-z]\d{9,10}|\d{9})$', N'9 Numeric or 1 Alpha and 9-10 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'OR', N'^\d{1,7}$', N'1-7 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'PA', N'^\d{8}$', N'8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'RI', N'^[\dV]\d{6}$', N' 1 Numeric or "V", 6 Numeric ')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'SC', N'^\d{6,9}$', N'6-9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'SD', N'^\d{8,9}$', N'8-9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'TN', N'^\d{7,9}$', N'7-9 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'TX', N'^\d{8}$', N'8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'UT', N'^\d{4,10}$', N'4-10 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'VA', N'^(\d{9}|[A-Z]\d{8})$', N'9 Numeric or 1 Alpha, 8 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'VT', N'^\d{7}[\dA]$', N'8 Numeric or 7 Numeric, "A"')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'WA', N'^[A-Z,*]{7}\d{3}[A-Z,0-9]{2}$', N'7 Alpha or *, 3 Numeric, 2 Alpha')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'WI', N'^[A-Z]\d{13}$', N'1 Alpha, 13 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'WV', N'^((0|A|B|C|D|E|F|S)\d{6}|([1|X)[X]\d{5}))$', N'Starts with "0" (thats a zero) or "A","B","C","D","E","F","S", 6 Numeric or Starts with ("1X" or "XX"), 5 Numeric')
INSERT [dbo].[DLPatterns] ([State], [Pattern], [Comment]) VALUES (N'WY', N'^\d{9,10}$', N'9-10 Numeric')
