USE [Common]
GO
/****** Object:  User [AppUser-Debug]    Script Date: 7/27/2014 2:06:52 PM ******/
CREATE USER [AppUser-Debug] FOR LOGIN [AppUser-Debug] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [AppUser-Debug]
GO
