USE [pgmMaster]
GO
/****** Object:  User [AppUser]    Script Date: 7/27/2014 4:24:39 PM ******/
CREATE USER [AppUser] FOR LOGIN [AppUser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [AppUser]
GO
