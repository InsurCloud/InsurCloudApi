USE [Common]
GO
/****** Object:  User [AppUser-Debug2]    Script Date: 7/27/2014 2:06:52 PM ******/
CREATE USER [AppUser-Debug2] FOR LOGIN [AppUser-Debug2] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [AppUser-Debug2]
GO
