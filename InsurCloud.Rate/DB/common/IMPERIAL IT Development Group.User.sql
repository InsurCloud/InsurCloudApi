USE [Common]
GO
/****** Object:  User [IMPERIAL\IT Development Group]    Script Date: 7/27/2014 2:06:52 PM ******/
CREATE USER [IMPERIAL\IT Development Group] FOR LOGIN [IMPERIAL\IT Development Group]
GO
ALTER ROLE [db_datareader] ADD MEMBER [IMPERIAL\IT Development Group]
GO
