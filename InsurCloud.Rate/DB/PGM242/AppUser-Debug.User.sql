USE [pgm242]
GO
/****** Object:  User [AppUser-Debug]    Script Date: 7/26/2014 4:28:10 PM ******/
CREATE USER [AppUser-Debug] FOR LOGIN [AppUser-Debug] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [AppUser-Debug]
GO
